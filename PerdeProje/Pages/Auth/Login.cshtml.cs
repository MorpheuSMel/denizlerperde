using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using PerdeProje.Data;
using PerdeProje.Services;
using System.Linq;

namespace PerdeProje.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; } = "";

        [BindProperty]
        public string Sifre { get; set; } = "";

        public string ErrorMessage { get; set; } = "";

        public void OnGet()
        {
            ErrorMessage = "";
        }

        public IActionResult OnPost()
        {
            // E-posta kontrolü (Boşlukları temizle)
            string temizEmail = Email?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(temizEmail) || string.IsNullOrWhiteSpace(Sifre))
            {
                ErrorMessage = "E-posta ve şifre boş bırakılamaz.";
                return Page();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == temizEmail.ToLower());

            if (user == null)
            {
                ErrorMessage = "❌ Bu e-posta ile kayıtlı kullanıcı bulunamadı.";
                return Page();
            }

            // Şifre kontrolü (Hem düz metin hem de şifrelenmiş (Hash) kontrolü)
            bool sifreDogruMu = false;

            // 1. İhtimal: Şifre veritabanına SQL'den dümdüz "123456" olarak eklenmişse
            if (user.Sifre == Sifre)
            {
                sifreDogruMu = true;
            }
            // 2. İhtimal: Şifre site üzerinden kayıt olunarak şifrelenmiş (Hash) ise
            else if (PasswordHasher.VerifyPassword(Sifre, user.Sifre))
            {
                sifreDogruMu = true;
            }

            if (!sifreDogruMu)
            {
                ErrorMessage = "❌ Şifre hatalı. Lütfen tekrar deneyin.";
                return Page();
            }

            // Giriş Başarılı - Session Tanımlamaları
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserEmail", user.Email ?? "");
            HttpContext.Session.SetString("UserName", $"{user.Ad} {user.Soyad}");
            HttpContext.Session.SetString("UserRole", user.Rol ?? "User");

            // Rolü kontrol ederken büyük/küçük harf duyarlılığını ortadan kaldırıyoruz
            // Admin ise admin paneline, normal kullanıcı ise kendi profiline yönlendir
            if (!string.IsNullOrEmpty(user.Rol) && user.Rol.Trim().ToLower() == "admin")
            {
                return RedirectToPage("/Admin/Dashboard");
            }
            else
            {
                // Kullanıcı Admin değilse, doğrudan KullaniciSayfasi'na yönlendir
                return RedirectToPage("/KullaniciSayfasi");
            }


        }
    }
}