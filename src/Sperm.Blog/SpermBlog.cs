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
            return Html("Hello World");
        }

        [Get("/totally")]
        public TextResult Hello2()
        {
            return Html("Hello 2 World");
        }
    }
}
