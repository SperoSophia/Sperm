using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm
{
    public class AuthorizeAttribute : Attribute
    {
        public bool NeedAuthorize { get; set; }

        public AuthorizeAttribute()
        {
            this.NeedAuthorize = true;
        }

        public AuthorizeAttribute(bool needAuthorize)
        {
            this.NeedAuthorize = needAuthorize;
        }
    }
}
