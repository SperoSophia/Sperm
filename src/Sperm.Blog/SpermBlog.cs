using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm.Blog
{
    public class Post
    {
        public string Title { get; set; }
        public string Body { get; set; }
    }

    [BaseUrl("")]
    public class SpermBlog : Sperm
    {
        public SpermBlog() { }

        [Get("/")]
        public TextResult Hello()
        {
            return Html("Hello World");
        }

        [Get("/blog")]
        public TextResult Hello2()
        {
            Post p = new Post { Title = "Test Title", Body = "Testing the body as well." };
            return View("Post", p);
        }
    }
}
