using Microsoft.AspNetCore.Mvc;
using RemoteContext = System.Web.HttpContext;
using ClassLibrary;

namespace MvcApp.Controllers;

public class AspNetCoreSessionController : Controller
{
    // GET: AspNetCoreSession
    public ActionResult Index()
    {
        var model = RemoteContext.Current?.Session?["SampleSessionItem"] as SessionDemoModel;

        return View(model);
    }

    // POST: AspNetCoreSession
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(SessionDemoModel demoModel)
    {
        if (RemoteContext.Current?.Session is not null)
        {
            RemoteContext.Current.Session["SampleSessionItem"] = demoModel;
        }

        return View(demoModel);
    }
}
