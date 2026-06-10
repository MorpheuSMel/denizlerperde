using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace PerdeProje.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // 🌟 Oturum (Session) hafızasındaki tüm kullanıcı verilerini temizler
            HttpContext.Session.Clear();

            // Kullanıcıyı güvenli bir şekilde ana sayfaya yönlendirir
            return RedirectToPage("/Index");
        }
    }
}