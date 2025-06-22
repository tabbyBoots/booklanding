using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace mvcDapper3.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Mis,User")]
    public class HomeController : Controller
    {
        // GET: HomeController
        public ActionResult Index()
        {
            return View();
        }
    }
}
