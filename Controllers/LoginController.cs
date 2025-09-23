using Microsoft.AspNetCore.Mvc;

namespace LoginAPI.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
