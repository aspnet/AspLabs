using Microsoft.AspNetCore.Mvc;
using ClassLibrary;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace MvcApp.Controllers;

[Session]
public class AspNetCoreSessionController : Controller
{
    // GET: AspNetCoreSession
    public ActionResult Index()
    {
        var model = System.Web.HttpContext.Current?.Session?["SampleSessionItem"] as SessionDemoModel;

        return View(model);
    }

    // POST: AspNetCoreSession
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(SessionDemoModel demoModel)
    {
        if (System.Web.HttpContext.Current?.Session is not null)
        {
            System.Web.HttpContext.Current.Session["SampleSessionItem"] = demoModel;
        }

        return View(demoModel);
    }
}
