using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace Sperm
{
    /*public class SperManager
    {
        public static SperManager global = new SperManager();

        
    }*/

    public static class PluginArchitecture
    {
        // List of interface our plugin accepts from external libraries.
        public static List<Type> AcceptedInterfaces { get; set; } = new List<Type>() {
            typeof(ISperm),
        };

        public static bool HasCoreImplementation(this Type self)
        {
            return self.GetInterfaces().Any(x => AcceptedInterfaces.Contains(x));
        }

        public static Type GetCoreImplementationType(this Type self)
        {
            return self.GetInterfaces().FirstOrDefault(x => AcceptedInterfaces.Contains(x));
        }
    }
    public enum IoCLifeTime { Singleton, MultiInstance }
    public class IoCRegistry
    {
        public string Name { get; set; }
        public Type Contract { get; set; }
        public Type Implementation { get; set; }
        public IoCLifeTime LifeTime { get; set; }
    }

    public static class IoCExtensions
    {
        public static IoCRegistry AsSingleton(this IoCRegistry self)
        {
            self.LifeTime = IoCLifeTime.Singleton;
            return self;
        }

        public static IoCRegistry AsMultiInstance(this IoCRegistry self)
        {
            self.LifeTime = IoCLifeTime.MultiInstance;
            return self;
        }
    }

    public static class SperManager
    {
        /// <summary>
        /// A dataset of the implementations of contracts.
        /// </summary>
        static readonly List<IoCRegistry> implementations = new List<IoCRegistry>();

        static List<object> instances = new List<object>();

        #region Register
        /// <summary>
        /// Register an implementation to a contract
        /// </summary>
        /// <typeparam name="TContract"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <returns></returns>
        public static IoCRegistry Register<TContract, TImplementation>()
        {
            return Register(typeof(TContract), typeof(TImplementation), "", IoCLifeTime.Singleton);
        }
        /// <summary>
        /// Register a named implementation of a contract
        /// </summary>
        /// <typeparam name="TContract"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static IoCRegistry Register<TContract, TImplementation>(string Name)
        {
            return Register(typeof(TContract), typeof(TImplementation), Name, IoCLifeTime.Singleton);
        }
        /// <summary>
        /// Register a named implentation of a contract with name and lifetime
        /// </summary>
        /// <typeparam name="TContract"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="Name"></param>
        /// <param name="LifeTime"></param>
        /// <returns></returns>
        public static IoCRegistry Register<TContract, TImplementation>(string Name, IoCLifeTime LifeTime)
        {
            return Register(typeof(TContract), typeof(TImplementation), Name, LifeTime);
        }
        /// <summary>
        /// Register and calibrated an Implementation to a Contract 
        /// </summary>
        /// <param name="Contract"></param>
        /// <param name="Implementation"></param>
        /// <param name="Name"></param>
        /// <param name="LifeTime"></param>
        /// <returns></returns>
        public static IoCRegistry Register(Type Contract, Type Implementation, string Name, IoCLifeTime LifeTime)
        {
            IoCRegistry result = new IoCRegistry
            {
                Name = Name,
                Contract = Contract,
                Implementation = Implementation,
                LifeTime = LifeTime
            };
            implementations.Add(result);
            return result;
        }

        public static IoCRegistry[] Register(Assembly asm)
        {
            List<IoCRegistry> result = new List<IoCRegistry>();
            IEnumerable<Type> types = asm.GetTypes().Where(x => x.HasCoreImplementation());
            foreach (Type t in types)
            {
                Type CoreType = t.GetCoreImplementationType();
                result.Add(Register(CoreType, t, "", IoCLifeTime.Singleton));
            }
            return result.ToArray();
        }
        public static IoCRegistry[] Register(string filename) { return Register(Assembly.Load(filename)); }

        #endregion
        #region Resolve
        /// <summary>
        /// Resolve a Contract to a Implementation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        /// <summary>
        /// Resolve the contract using its constructor
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public static object Resolve(Type contract)
        {
            IoCRegistry ResolutionType = implementations.FirstOrDefault(x => x.Contract == contract);

            // if single instance
            if (ResolutionType.LifeTime == IoCLifeTime.Singleton)
            {
                object instance = instances.FirstOrDefault(x => x.GetType() == ResolutionType.Implementation);
                if (instance != null)
                    return instance;
            }

            Type implementation = ResolutionType.Implementation;
            // Get the constructor with most parameters to make sure we get utilize the interfaces full potential.
            ConstructorInfo constructor = implementation.GetConstructors().OrderByDescending(x => x.GetParameters()).First();
            ParameterInfo[] constructorParameters = constructor.GetParameters();
            if (constructorParameters.Length == 0)
            {
                return Activator.CreateInstance(implementation);
            }

            List<object> parameters = new List<object>(constructorParameters.Length);
            foreach (ParameterInfo parameterInfo in constructorParameters)
            {
                parameters.Add(Resolve(parameterInfo.ParameterType));
            }

            object inst = constructor.Invoke(parameters.ToArray());

            // only add singleton instances, so we dont get an overflow of potentially millions of multi-instance over an application runtime.
            if (ResolutionType.LifeTime == IoCLifeTime.Singleton)
            {
                instances.Add(inst);
            }

            return inst;
        }
        #endregion
    }
}
