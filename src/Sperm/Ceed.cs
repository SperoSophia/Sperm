using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Sperm
{
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

    public static class Ceed
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
        //public static IoCRegistry[] Register(string filename) { return Register(Assembly.Load(filename)); } // [BUG] .NET Core Restrictions.

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

        public static IEnumerable<T> ResolveMany<T>()
        {
            IEnumerable<IoCRegistry> ResolutionTypes = implementations.Where(x => x.Contract == typeof(T));
            List<T> result = new List<T>();
            foreach(IoCRegistry r in ResolutionTypes)
            {
                result.Add((T)Resolve(r));
            }
            return result;
        }

        public static object Resolve(Type contract)
        {
            IoCRegistry ResolutionType = implementations.FirstOrDefault(x => x.Contract == contract);
            return Resolve(ResolutionType);
        }

        /// <summary>
        /// Resolve the contract using its constructor
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public static object Resolve(IoCRegistry ResolutionType)
        {
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

    public static class NETCoreAssemblyExtension
    {
        public static IEnumerable<Assembly> GetReferencingAssemblies(this Assembly self)
        {
            return GetReferencingAssemblies(self.FullName);
        }

        public static IEnumerable<Assembly> GetReferencingAssemblies(string assemblyName)
        {
            var assemblies = new List<Assembly>();
            var dependencies = DependencyContext.Default.RuntimeLibraries;
            foreach (var library in dependencies)
            {
                if (IsCandidateLibrary(library, assemblyName))
                {
                    var assembly = Assembly.Load(new AssemblyName(library.Name));
                    assemblies.Add(assembly);
                }
            }
            var SpermAssembly = typeof(ISperm).GetTypeInfo().Assembly;
            if(!assemblies.Any(x => x.FullName == SpermAssembly.FullName))
            {
                assemblies.Add(SpermAssembly);
            }
            return assemblies;
        }

        public static bool IsCandidateLibrary(RuntimeLibrary library, string assemblyName)
        {
            return library.Name == (assemblyName) || library.Dependencies.Any(d => d.Name == "Sperm");
        }

        public static Type[] AllTypesOf(this Assembly self, Type baseType)
        {
            return self.GetReferencingAssemblies().SelectMany(s => s.GetTypes()).Where(p => baseType.IsAssignableFrom(p) && !p.GetTypeInfo().IsAbstract).ToArray();
        }
    }
}
