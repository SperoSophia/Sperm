using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm
{
    public class PutAttribute : HttpVerbAttribute
    {
        public override string Verb { get { return "PUT"; } }
        public PutAttribute(string path) : base(path) { }
    }
}
