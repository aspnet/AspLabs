using System.Web.Mvc;
using ClassLibrary;

namespace MvcApp.Controllers
{
    public class AspNetSessionController : Controller
    {
        // GET: AspNetCoreSession
        public ActionResult Index()
        {
            var model = HttpContext.Session["SampleSessionItem"] as SessionDemoModel;

            return View(model);
        }

        // POST: AspNetCoreSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SessionDemoModel demoModel)
        {
            HttpContext.Session["SampleSessionItem"] = demoModel;

            return View(demoModel);
        }
    }
}
