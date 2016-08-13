

using Microsoft.AspNetCore.Http;
using Sperm.Utils;

namespace Sperm
{
    public class JsonResult : BaseResult
    {
        private object _result;

        public JsonResult(object result)
        {
            this.ContentType = "application/json";
            _result = result;
        }

        public override void Execute(HttpContext context)
        {
            base.Execute(context);

            if (_result == null)
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes("null");
                context.Response.Body.Write(buffer, 0, buffer.Length);
            }
            else
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(_result.ToJson());
                context.Response.Body.Write(buffer, 0, buffer.Length);
            }
        }
    }
}