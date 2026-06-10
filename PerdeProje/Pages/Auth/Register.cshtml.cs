using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PerdeProje.Data;
using PerdeProje.Models;
using PerdeProje.Services;

namespace PerdeProje.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Ad { get; set; }

        [BindProperty]
        public string Soyad { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Telefon { get; set; }

        [BindProperty]
        public string Sifre { get; set; }

        [BindProperty]
        public string SifreKontrol { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public void OnGet()
        {
            // Eğer kullanıcı zaten giriş yapmışsa, ana sayfaya yönlendir
            if (HttpContext.Session.GetString("UserId") != null)
            {
                RedirectToPage("/Index");
            }
        }

        public IActionResult OnPost()
        {
            // Boş alan kontrol
            if (string.IsNullOrWhiteSpace(Ad) || string.IsNullOrWhiteSpace(Soyad) || 
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Sifre) ||
                string.IsNullOrWhiteSpace(SifreKontrol))
            {
                ErrorMessage = "Lütfen tüm zorunlu alanları (* ile işaretlenenler) doldurunuz.";
                return Page();
            }

            // Şifre ve şifre tekrar kontrol
            if (Sifre != SifreKontrol)
            {
                ErrorMessage = "❌ Şifreler eşleşmiyor! Lütfen aynı şifreyi girdiğinizden emin olun.";
                return Page();
            }

            // Şifre uzunluğu kontrol
            if (Sifre.Length < 6)
            {
                ErrorMessage = "❌ Şifre en az 6 karakter olmalıdır. Daha uzun bir şifre girin.";
                return Page();
            }

            var temizTelefon = TelefonRakamlari(Telefon);
            if (!string.IsNullOrWhiteSpace(Telefon) && (temizTelefon.Length != 11 || !temizTelefon.StartsWith("0")))
            {
                ErrorMessage = "Telefon numarasi 0 ile baslayan 11 haneli olmalidir.";
                return Page();
            }

            try
            {
                // E-posta zaten kayıtlı mı kontrol et
                if (_context.Users.Any(u => u.Email == Email))
                {
                    ErrorMessage = "❌ Bu e-posta adresi zaten kayıtlı. Başka bir e-posta kullanın.";
                    return Page();
                }

                // Yeni kullanıcı oluştur
                var user = new User
                {
                    Ad = Ad.Trim(),
                    Soyad = Soyad.Trim(),
                    Email = Email.Trim(),
                    Telefon = TelefonuFormatla(temizTelefon),
                    Sifre = PasswordHasher.HashPassword(Sifre),
                    Rol = "User", // Varsayılan rol User
                    OlusturmaTarihi = DateTime.Now,
                    AktifMi = true
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                // Başarılı kayıt, login sayfasına yönlendir
                SuccessMessage = "✅ Kayıt başarılı! Lütfen giriş yapınız.";
                return RedirectToPage("Login");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"❌ Kayıt sırasında hata oluştu: {ex.InnerException?.Message ?? ex.Message}";
                return Page();
            }
        }

        private static string TelefonRakamlari(string? telefon)
        {
            return new string((telefon ?? string.Empty).Where(char.IsDigit).Take(11).ToArray());
        }

        private static string TelefonuFormatla(string? telefon)
        {
            var rakamlar = TelefonRakamlari(telefon);
            if (rakamlar.Length != 11)
            {
                return rakamlar;
            }

            return $"{rakamlar[..4]} {rakamlar.Substring(4, 3)} {rakamlar.Substring(7, 2)} {rakamlar.Substring(9, 2)}";
        }
    }
}
