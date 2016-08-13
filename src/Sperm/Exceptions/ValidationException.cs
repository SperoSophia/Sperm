using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(string.Format("HttpRequestValidation Error: {0}", message)) { }
    }
}
