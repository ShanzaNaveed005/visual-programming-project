using Microsoft.AspNetCore.Mvc;

namespace AITourismPlanner.Controllers
{
    public class ChatbotController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}