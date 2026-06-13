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
        public string Ad { get; set; } = "";

        [BindProperty]
        public string Soyad { get; set; } = "";

        [BindProperty]
        public string Email { get; set; } = "";

        [BindProperty]
        public string Telefon { get; set; } = "";

        [BindProperty]
        public string Sifre { get; set; } = "";

        [BindProperty]
        public string SifreKontrol { get; set; } = "";

        public string ErrorMessage { get; set; } = "";
        public string SuccessMessage { get; set; } = "";

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Ad) ||
                string.IsNullOrWhiteSpace(Soyad) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Sifre) ||
                string.IsNullOrWhiteSpace(SifreKontrol))
            {
                ErrorMessage = "Lütfen tüm zorunlu alanları doldurun.";
                return Page();
            }

            if (Sifre != SifreKontrol)
            {
                ErrorMessage = "Şifreler eşleşmiyor. Lütfen aynı şifreyi girin.";
                return Page();
            }

            if (Sifre.Length < 6)
            {
                ErrorMessage = "Şifre en az 6 karakter olmalıdır.";
                return Page();
            }

            var temizTelefon = TelefonRakamlari(Telefon);
            if (!string.IsNullOrWhiteSpace(Telefon) && !GecerliTelefonMu(temizTelefon))
            {
                ErrorMessage = "Telefon numarası 05xx xxx xx xx formatında olmalıdır.";
                return Page();
            }

            try
            {
                if (_context.Users.Any(u => u.Email == Email.Trim()))
                {
                    ErrorMessage = "Bu e-posta adresi zaten kayıtlı. Başka bir e-posta kullanın.";
                    return Page();
                }

                var user = new User
                {
                    Ad = Ad.Trim(),
                    Soyad = Soyad.Trim(),
                    Email = Email.Trim(),
                    Telefon = TelefonuFormatla(temizTelefon),
                    Sifre = PasswordHasher.HashPassword(Sifre),
                    Rol = "User",
                    OlusturmaTarihi = DateTime.Now,
                    AktifMi = true
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                SuccessMessage = "Kayıt başarılı. Lütfen giriş yapın.";
                return RedirectToPage("Login");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Kayıt sırasında hata oluştu: {ex.InnerException?.Message ?? ex.Message}";
                return Page();
            }
        }

        private static string TelefonRakamlari(string? telefon)
        {
            var rakamlar = new string((telefon ?? string.Empty).Where(char.IsDigit).ToArray());
            if (rakamlar.Length == 10 && !rakamlar.StartsWith("0"))
            {
                rakamlar = "0" + rakamlar;
            }

            return rakamlar.Length > 11 ? rakamlar[..11] : rakamlar;
        }

        private static bool GecerliTelefonMu(string telefon)
        {
            if (string.IsNullOrWhiteSpace(telefon))
            {
                return true;
            }

            if (telefon.Length != 11 || !telefon.StartsWith("05"))
            {
                return false;
            }

            if (telefon.Distinct().Count() <= 2)
            {
                return false;
            }

            var kolayNumaralar = new[] { "05123456789", "05000000000", "05555555555", "05333333333" };
            return !kolayNumaralar.Contains(telefon);
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
