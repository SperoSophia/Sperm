using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeus
{
    public class BaseResult
    {
        public static string Cors;

        public string ContentType { get; set; }
        public int? StatusCode { get; set; }
        public Dictionary<string, string> Header { get; set; } = new Dictionary<string, string>();

        public BaseResult()
        {
            //this.Header = new NameValueCollection();
            this.ContentType = "application/json";
        }

        public virtual void Execute(HttpContext context)
        {
            //response.Headers.Clear();
            //response.Body.Flush();

            if (this.StatusCode.HasValue)
            {
                context.Response.StatusCode = StatusCode.Value;
            }

            foreach (string key in this.Header.Keys)
            {
                context.Response.Headers.Add(key, Header[key]);
            }

            context.Response.ContentType = ContentType;
        }
    }

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

            byte[] buffer = Encoding.UTF8.GetBytes(_text);
            context.Response.Body.Write(buffer, 0, buffer.Length); 
        }
    }
}
