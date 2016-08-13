using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm
{
    public interface IViewEngine
    {
        string Name { get; }
        string Render(string view, object model);
    }
}
