using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm
{
    public class HttpException : Exception
    {
        public HttpException(int code, string message) : base(string.Format("HttpCode: {0} - {1}", code, message)) { }
    }
}
