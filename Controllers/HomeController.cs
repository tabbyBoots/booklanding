
namespace mvcDapper3.Controllers;

public class HomeController : Controller
{
    [AllowAnonymous]
    public IActionResult Index()
    {
        // Role-based redirection logic
        if (User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Mis") || User.IsInRole("User"))
            {
                // Redirect admin roles to admin dashboard
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            // Member role continues to landing page
        }
        // Guests and Members see the landing page
        return View();
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }
}
