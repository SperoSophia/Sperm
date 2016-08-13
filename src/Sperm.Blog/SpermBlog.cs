using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm.Blog
{
    [BaseUrl("")]
    public class SpermBlog : Sperm
    {
        public SpermBlog() { }

        [Get("/")]
        public TextResult Hello()
        {
            return new TextResult("Hello World");
        }
    }
}
