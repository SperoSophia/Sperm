using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm
{
    public class DeleteAttribute : HttpVerbAttribute
    {
        public override string Verb { get { return "DELETE"; } }
        public DeleteAttribute(string path) : base(path) { }
    }
}
