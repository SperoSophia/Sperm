using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm
{
    public class PostAttribute : HttpVerbAttribute
    {
        public override string Verb { get { return "POST"; } }
        public PostAttribute(string path) : base(path) { }
    }
}
