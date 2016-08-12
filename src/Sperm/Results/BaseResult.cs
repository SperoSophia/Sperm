using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sperm
{
    public class BaseResult
    {
        public static string Cors;

        public string ContentType { get; set; } = "application/json";
        public int? StatusCode { get; set; }
        public Dictionary<string, string> Header { get; set; } = new Dictionary<string, string>();

        public BaseResult() { }

        public virtual void Execute(HttpContext context)
        {
            var response = context.Response;

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
}