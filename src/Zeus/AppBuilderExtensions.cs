using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zeus
{
    public class ZeusMiddleWare
    {
        private readonly RequestDelegate _next;

        public ZeusMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var obj = new ZeusController().Html("Test html");

            // if result is BaseResult, use it, otherwise, convert to JsonResult
            var result = obj; // is BaseResult ? (BaseResult)obj : new JsonResult(obj);

            result.Execute(context);
        }
    }

    public static class AppBuilderExtensions
    {

        public static IApplicationBuilder UseZeus(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ZeusMiddleWare>();
        }

    }
}
