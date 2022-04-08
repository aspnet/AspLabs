using MvcApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Web.Adapters;
using RemoteContext = System.Web.HttpContext;

namespace MvcApp.Controllers;

[Session]
public class AspNetCoreSessionController : Controller
{
    // GET: AspNetCoreSession
    public ActionResult Index()
    {
        var model = new SessionDemoModel
        {
            IntSessionItem = RemoteContext.Current?.Session?[SessionDemoModel.IntSessionItemName] as int?,
            StringSessionItem = RemoteContext.Current?.Session?[SessionDemoModel.StringSessionItemName] as string
        };

        return View(model);
    }

    // POST: AspNetCoreSession
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index(SessionDemoModel demoModel)
    {
        if (RemoteContext.Current?.Session is not null)
        {
            if (demoModel.IntSessionItem.HasValue)
            {
                RemoteContext.Current.Session[SessionDemoModel.IntSessionItemName] = demoModel.IntSessionItem.Value;
            }
            else
            {
                RemoteContext.Current.Session.Remove(SessionDemoModel.IntSessionItemName);
            }

            RemoteContext.Current.Session[SessionDemoModel.StringSessionItemName] = demoModel.StringSessionItem;
        }

        return View(demoModel);
    }
}
