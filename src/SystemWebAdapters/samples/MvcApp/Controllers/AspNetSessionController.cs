using System.Web.Mvc;
using MvcApp.Models;

namespace MvcApp.Controllers
{
    public class AspNetSessionController : Controller
    {
        // GET: AspNetCoreSession
        public ActionResult Index()
        {
            var model = new SessionDemoModel
            {
                IntSessionItem = HttpContext.Session[SessionDemoModel.IntSessionItemName] as int?,
                StringSessionItem = HttpContext.Session[SessionDemoModel.StringSessionItemName] as string,
            };

            return View(model);
        }

        // POST: AspNetCoreSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SessionDemoModel demoModel)
        {
            if (demoModel.IntSessionItem.HasValue)
            {
                HttpContext.Session[SessionDemoModel.IntSessionItemName] = demoModel.IntSessionItem.Value;
            }
            else
            {
                HttpContext.Session.Remove(SessionDemoModel.IntSessionItemName);
            }

            HttpContext.Session[SessionDemoModel.StringSessionItemName] = demoModel.StringSessionItem;

            return View(demoModel);
        }
    }
}
