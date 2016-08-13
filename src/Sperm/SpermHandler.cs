﻿/* SPERM - Surprisingly Practical Extendable Rest Module */
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Principal;
using System.IO;
using Sperm.Utils;

namespace Sperm
{
    public class SpermHandler
    {
        private readonly RequestDelegate _next;

        internal static List<RouteInfo> Routes { get; set; }

        public SpermHandler(RequestDelegate next) 
        {
            _next = next;
        }

        public void Initialize()
        {
            var routes = Routes = new List<RouteInfo>();
            var baseType = typeof(ISperm);
            var types = new List<object>(); // Assembly.GetEntryAssembly().GetReferencedAssemblies().Where(x => x.ProcessorArchitecture); 
                /*DependencyContext Assembly.GetEntryAssembly().GetReferencedAssemblies CurrentDomain.GetAssemblies().ToList()
                .SelectMany(s => s.GetTypes())
                .Where(p => baseType.IsAssignableFrom(p) && !p.IsAbstract);*/

            /*foreach (var type in types)
            {
                var authorize = (AuthorizeAttribute)type.GetCustomAttributes(typeof(AuthorizeAttribute), true).FirstOrDefault();
                var baseurl = (BaseUrlAttribute)type.GetCustomAttributes(typeof(BaseUrlAttribute), true).FirstOrDefault();

                foreach (var method in type.GetMethods().Where(x => x.IsPublic))
                {
                    var attr = (HttpVerbAttribute)method.GetCustomAttributes(typeof(HttpVerbAttribute), true).FirstOrDefault();

                    if (attr != null)
                    {
                        var pars = method.GetParameters();

                        var route = new RouteInfo(_mountedUrl, baseurl == null ? "" : baseurl.Path, attr.Path, pars);
                        route.Verb = attr.Verb;
                        route.Module = type;
                        route.MethodInfo = method;
                        route.Authorize = authorize != null && authorize.NeedAuthorize;
                        route.Validate = true;

                        // check for authorize attribute - in class or in method
                        var mauthorize = (AuthorizeAttribute)method.GetCustomAttributes(typeof(AuthorizeAttribute), true).FirstOrDefault();

                        if (mauthorize != null)
                        {
                            route.Authorize = mauthorize.NeedAuthorize;
                        }

                        var mvalidate = (ValidateInputAttribute)method.GetCustomAttributes(typeof(ValidateInputAttribute), true).FirstOrDefault();

                        if (mvalidate != null)
                        {
                            route.Validate = mvalidate.Validate;
                        }

                        // get roles attributes
                        var roles = (RoleAttribute)method.GetCustomAttributes(typeof(RoleAttribute), true).FirstOrDefault();

                        route.Roles = roles != null ? roles.Roles : new string[0];

                        routes.Add(route);
                    }
                }
            }*/
        }

        public async Task Invoke(HttpContext context)
        {
            var obj = new SperModule().Html("Test html");

            // if result is BaseResult, use it, otherwise, convert to JsonResult
            var result = obj; // is BaseResult ? (BaseResult)obj : new JsonResult(obj);

            result.Execute(context);
        }
    }

    internal class RouteInfo
    {
        public Regex Pattern { get; set; }
        public string Path { get; set; }
        public string Verb { get; set; }
        public string[] Roles { get; set; }
        public bool Authorize { get; set; }
        public bool Validate { get; set; }

        public string RegExPattern { get; set; }

        public MethodInfo MethodInfo { get; set; }
        public string MethodName { get { return MethodInfo.Name + "(" + string.Join(", ", MethodInfo.GetParameters().Select(x => x.ParameterType.Name + " " + x.Name)) + ")"; } }
        public Type Module { get; set; }

        public RouteInfo(string mounted, string baseurl, string path, ParameterInfo[] pars)
        {
            var text = Regex.Replace(baseurl + path, @"^(.+)/$", "$1");

            this.Path = mounted + text;

            // creating regex por parameters
            foreach (var par in pars)
            {
                var name = par.Name;
                var expr = @"(?<$1>[^/]+)";

                // if parameter is a int, lets add regex (avoid 
                if (par.ParameterType == typeof(Int16) || par.ParameterType == typeof(Int32) || par.ParameterType == typeof(Int64)) // Inteiro
                {
                    expr = @"(?<$1>\d+)";
                }
                text = Regex.Replace(text, @"\{(" + name + @")\}", expr);

                // used for regex expression on route definition.
                // "/pubdate/{year:(\\d{4})}/{month:(\\d{2})"
                // "/code/{id:(\\[a-z]{8})}"
                // Syntax: "/demo/{parameterName:(regularExpression)}"

                text = Regex.Replace(text, @"\{(" + name + @"):\((.+?)\)\}", @"(?<$1>$2)");
            }

            this.RegExPattern = "^" + text + "$";
            this.Pattern = new Regex(this.RegExPattern);
        }

        public void ValidateAuthorizeAndRoles(IPrincipal user)
        {
            // lets check if user is authenticated
            if (this.Authorize && user.Identity.IsAuthenticated == false)
            {
                throw new HttpException(401, "Unauthorized");
            }

            // lets check if user has role for this method
            foreach (var role in this.Roles)
            {
                if (user.IsInRole(role))
                {
                    break;
                }
                throw new HttpException(403, "Forbidden");
            }
        }

        public object[] GetMethodParameters(Match match, HttpContext context)
        {
            var args = new List<object>();
            string model = null;

            // Parameters bind rules:
            // - if found on route pattern? Use route value
            // - if not found:
            //    - Parameter is `String`? Use `Request.Form.ToString()`
            //    - Parameter is `NameValueCollection`? Use `Request.Params` = Form + QueryString + Cookies + ServerVariables
            //    - Parameter is `Stream`? Use `Request.InputStream`
            //    - Parameter is `IPrincipal`? Use `HttpContext.User`
            //    - Parameter is `HttpContext`? Use `HttpContext`
            //    - Parameter is `HttpContextWrapper`? Use `new HttpContextWrapper(context)`
            //    - Parameter is `JObject`? Use `JObject.Parse()`
            //    - Parameter any other type? Use `JsonDeserialize(model, parameterType)`
            //    - Model is `Null`? Use `default(T)`

            // [Post("/order/{id}"]
            // void Edit(int id, OrderModel order, NameValueCollection form) { ... }

            foreach (ParameterInfo p in MethodInfo.GetParameters())
            {
                var pmatch = match.Groups[p.Name];

                if (pmatch.Success)
                {
                    var value = pmatch.Value;
                    args.Add(Convert.ChangeType(value, p.ParameterType));
                }
                else
                {
                    var request = context.Request;

                    if (p.ParameterType == typeof(Stream)) // bind Stream to Request.InputStream
                    {
                        args.Add(request.Body);
                    }
                    else if (p.ParameterType == typeof(IPrincipal)) // bind as Context.User
                    {
                        args.Add(context.User);
                    }
                    else if (p.ParameterType == typeof(HttpContext)) // bind as context
                    {
                        args.Add(context);
                    }
                    else 
                    {
                        if (model == null) // load request as string
                        {
                            model = this.GetModel(request.Body);
                        }

                        if (p.ParameterType == typeof(string)) // bind model as string 
                        {
                            args.Add(model);
                        }
                        else if (model == "") // no model in a custom parameter type
                        {
                            args.Add(GetDefault(p.ParameterType));
                        }
                        if (p.ParameterType == typeof(object)) // bind model as string 
                        {
                            args.Add(model.FromJson<object>());
                        }
                        else
                        {
                            throw new HttpException(500, "Unknown Request Body Type.");
                        }
                    }
                }
            }

            return args.Count == 0 ? null : args.ToArray();
        }

        private string GetModel(Stream stream)
        {
            if (this.Verb == "GET") return null;

            var model = "";

            using (var reader = new StreamReader(stream))
            {
                model = HttpUtility.Decode(reader.ReadToEnd());
            }

            if (string.IsNullOrWhiteSpace(model)) model = "";

            // lets validate request
            if (this.Validate && model != "" && Regex.IsMatch(model, @"^(?!(.|\n)*<[a-z!\/?])(?!(.|\n)*&#)(.|\n)*$", RegexOptions.IgnoreCase) == false)
            {
                throw new ValidationException("A potentially dangerous request value was detected from the client");
            }

            return model;
        }

        private static object GetDefault(Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }

    public static class SpermHandlerExtensions
    {

        public static IApplicationBuilder UseSperm(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SpermHandler>();
        }

    }
}
