using Microsoft.AspNetCore.Mvc;

namespace Motorbike.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}