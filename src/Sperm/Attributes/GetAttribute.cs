using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm
{
    public class GetAttribute : HttpVerbAttribute
    {
        public override string Verb { get { return "GET"; } }
        public GetAttribute(string path) : base(path) { }
    }
}
