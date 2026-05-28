using Microsoft.AspNetCore.Mvc;

namespace AITourismPlanner.Controllers
{
    public class TransportsController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Message = "Transport booking system coming soon!";
            return View();
        }
    }
}