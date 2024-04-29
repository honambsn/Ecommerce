using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class HangHoaController : Controller
    {
        public IActionResult Index(int? loai)
        {
            return View();
        }
    }
}
