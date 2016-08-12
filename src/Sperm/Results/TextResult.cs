using System.IO;
using Microsoft.AspNetCore.Http;

namespace Sperm
{
    public class TextResult : BaseResult
    {
        private readonly string _text;

        public TextResult(string text, string contentType = "plain/text")
        {
            this.ContentType = contentType;
            _text = text;
        }

        public override void Execute(HttpContext context)
        {
            base.Execute(context);

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(_text);
            context.Response.Body.Write(buffer, 0, buffer.Length);
        }
    }
}