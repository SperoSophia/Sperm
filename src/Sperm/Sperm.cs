using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Sperm
{
    public interface ISperm : IDisposable
    {
        void OnException(Exception ex);
        void OnExecuting(MethodInfo methodInfo, object[] args);
        void OnExecuted(MethodInfo methodInfo, BaseResult result);
        BaseResult Ok();
        BaseResult Status(HttpStatusCode statusCode);
        BaseResult Status(int statusCode);
        TextResult Html(string html);
        TextResult View(string view, object model);
    }

    public partial class Sperm : ISperm
    {
        public Sperm() { }
        public virtual void OnException(Exception ex) { }
        public virtual void OnExecuting(MethodInfo methodInfo, object[] args) { }
        public virtual void OnExecuted(MethodInfo methodInfo, BaseResult result) { }
        public virtual void Dispose() { }

        public BaseResult Ok()
        {
            return this.Status(HttpStatusCode.OK);
        }

        public BaseResult Status(HttpStatusCode statusCode)
        {
            var result = new BaseResult();
            result.StatusCode = (int)statusCode;
            return result;
        }

        public BaseResult Status(int statusCode)
        {
            var result = new BaseResult();
            result.StatusCode = statusCode;
            return result;
        }

        public TextResult Html(string html)
        {
            var result = new TextResult(html);
            result.StatusCode = (int)HttpStatusCode.OK;
            result.ContentType = "text/html";
            return result;
        }

        public TextResult View(string view, object model)
        {
            IEnumerable<IViewEngine> engines = Ceed.ResolveMany<IViewEngine>();
            string viewExtension = "";
            try
            {
                viewExtension = System.IO.Path.GetExtension(view);
            }
            catch { }
            if(!string.IsNullOrEmpty(viewExtension))
            {
                engines = engines.Where(x => x.EmbeededViews == false && x.Extension.Contains(viewExtension) == false);
            }
            var engine = engines.FirstOrDefault();
            if (engine == null)
                throw new Exception("No View Engines Registered");

            try
            {
                return Html(engine.Render(view, model));
            } catch
            {
                throw new Exception("Unable to render using the registered view engine. You might have to many engines registered at the same time.");
            }
        }
    }
}
