using System.Web.Mvc;

namespace SpaTemplate.Web.Framework.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}