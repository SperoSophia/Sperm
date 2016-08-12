/* SPERM - Surprisingly Practical Extendable Rest Module */
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Sperm
{
    public class SpermMiddleWare
    {
        private readonly RequestDelegate _next;

        public SpermMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var obj = new SperModule().Html("Test html");

            // if result is BaseResult, use it, otherwise, convert to JsonResult
            var result = obj; // is BaseResult ? (BaseResult)obj : new JsonResult(obj);

            result.Execute(context);
        }
    }

    public static class AppBuilderExtensions
    {

        public static IApplicationBuilder UseSperm(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SpermMiddleWare>();
        }

    }
}
