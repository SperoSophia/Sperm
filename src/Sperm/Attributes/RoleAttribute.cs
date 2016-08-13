using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm
{
    public class RoleAttribute : Attribute
    {
        public string[] Roles { get; set; }

        public RoleAttribute(params string[] roles)
        {
            this.Roles = roles;
        }
    }
}
