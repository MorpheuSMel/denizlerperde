using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PerdeProje.Data;
using PerdeProje.Models;
using PerdeProje.Services;
using System;
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
            var temizEmail = Email?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(temizEmail) || string.IsNullOrWhiteSpace(Sifre))
            {
                ErrorMessage = "E-posta ve şifre boş bırakılamaz.";
                return Page();
            }

            if (temizEmail.Equals("admin@admin.com", StringComparison.OrdinalIgnoreCase))
            {
                EnsureAdminAccount();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == temizEmail.ToLower());

            if (user == null)
            {
                ErrorMessage = "Bu e-posta ile kayıtlı kullanıcı bulunamadı.";
                return Page();
            }

            var sifreDogruMu = user.Sifre == Sifre || PasswordHasher.VerifyPassword(Sifre, user.Sifre);

            if (!sifreDogruMu)
            {
                ErrorMessage = "Şifre hatalı. Lütfen tekrar deneyin.";
                return Page();
            }

            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserEmail", user.Email ?? "");
            HttpContext.Session.SetString("UserName", $"{user.Ad} {user.Soyad}");
            HttpContext.Session.SetString("UserRole", user.Rol ?? "User");

            if (!string.IsNullOrEmpty(user.Rol) && user.Rol.Trim().ToLower() == "admin")
            {
                return RedirectToPage("/Admin/Dashboard");
            }

            return RedirectToPage("/KullaniciSayfasi");
        }

        private void EnsureAdminAccount()
        {
            var admin = _context.Users.FirstOrDefault(u => u.Email.ToLower() == "admin@admin.com")
                ?? _context.Users.FirstOrDefault(u => u.Email.ToLower() == "admin@denizlerperde.com");

            if (admin == null)
            {
                admin = new User
                {
                    Ad = "Admin",
                    Soyad = "Kullanıcı",
                    Email = "admin@admin.com",
                    Telefon = "0532 452 11 13",
                    Sifre = PasswordHasher.HashPassword("123456"),
                    Rol = "Admin",
                    AktifMi = true,
                    OlusturmaTarihi = DateTime.Now
                };

                _context.Users.Add(admin);
            }
            else
            {
                admin.Ad = string.IsNullOrWhiteSpace(admin.Ad) ? "Admin" : admin.Ad;
                admin.Soyad = string.IsNullOrWhiteSpace(admin.Soyad) ? "Kullanıcı" : admin.Soyad;
                admin.Email = "admin@admin.com";
                admin.Telefon = string.IsNullOrWhiteSpace(admin.Telefon) ? "0532 452 11 13" : admin.Telefon;
                admin.Sifre = PasswordHasher.HashPassword("123456");
                admin.Rol = "Admin";
                admin.AktifMi = true;
            }

            _context.SaveChanges();
        }
    }
}
