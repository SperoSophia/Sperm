using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm
{
    public abstract class HttpVerbAttribute : Attribute
    {
        public abstract string Verb { get; }
        public string Path { get; set; }

        public HttpVerbAttribute(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (!path.StartsWith("/")) throw new ArgumentException("Verb path must starts with / (ex: /view/{id})");
            if (path.Length > 1 && path.EndsWith("/")) throw new ArgumentException("Verb path must not ends with / (ex: /view/{id})");

            this.Path = path;
        }
    }
}
