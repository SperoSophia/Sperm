using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm
{
    public class BaseUrlAttribute : Attribute
    {
        public string Path { get; set; }

        public BaseUrlAttribute(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (!path.StartsWith("/") || path.EndsWith("/")) throw new ArgumentException("BaseUrl path must starts with / (ex: /customers)");
            if (path.Length > 1 && path.EndsWith("/")) throw new ArgumentException("BaseUrl path must not ends with a / (ex: /customers)");

            this.Path = path;
        }
    }
}
