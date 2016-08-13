using RazorLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sperm.RazorLight
{
    public class RazorLightEngine : IViewEngine
    {
        public RazorLightEngine() { }

        public bool EmbeededViews
        {
            get
            {
                return true;
            }
        }

        public IEnumerable<string> Extension
        {
            get
            {
                return new List<string>();
            }
        }

        public string Name
        {
            get
            {
                return "RazorLight";
            }
        }

        public string Render(string view, object model)
        {
            var engine = EngineFactory.CreateEmbedded(model.GetType());
            return engine.Parse(view, model);
        }
    }
}
