using Microsoft.AspNetCore.Mvc;

namespace MVCApplication.Controllers
{
    public class FileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
