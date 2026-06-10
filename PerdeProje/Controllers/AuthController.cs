using Microsoft.AspNetCore.Mvc;

namespace PerdeProje.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Logout()
        {
            // Session'ı temizle
            HttpContext.Session.Clear();

            // Çıkış mesajıyla ana sayfaya yönlendir
            return RedirectToAction("Index", "Home");
        }
    }
}
