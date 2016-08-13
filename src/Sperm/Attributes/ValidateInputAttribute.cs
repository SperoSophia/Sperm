using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm
{
    public class ValidateInputAttribute : Attribute
    {
        public bool Validate { get; set; }

        public ValidateInputAttribute()
        {
            this.Validate = true;
        }

        public ValidateInputAttribute(bool validate)
        {
            this.Validate = validate;
        }
    }
}
