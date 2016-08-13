using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm.Controllers
{
    [BaseUrl("/Sperm/Diagnostics")]
    public class Diagnostics : Sperm
    {
        [Get("/")]
        public BaseResult Get()
        {
            return Html("Test");
        }
    }
}
