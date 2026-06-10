using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using PerdeProje.Data;
using PerdeProje.Models;
using PerdeProje.Pages.Admin;
using PerdeProje.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerdeProje.Pages
{
    public class KullaniciSayfasiModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public KullaniciSayfasiModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Eposta { get; set; } = string.Empty;

        [BindProperty]
        public string Telefon { get; set; } = string.Empty;

        [BindProperty]
        public string? Sifre { get; set; }

        public string BilgiMesaji { get; set; } = string.Empty;

        public List<Siparis> KullaniciSiparisleri { get; set; } = new List<Siparis>();
        public List<Favori> KullaniciFavorileri { get; set; } = new List<Favori>();
        public List<Randevu> KullaniciRandevulari { get; set; } = new List<Randevu>();

        public IActionResult OnGet()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToPage("/Auth/Login");
            }

            int userId = int.Parse(userIdStr);

            var kullanici = _context.Set<User>().FirstOrDefault(k => k.Id == userId);
            if (kullanici != null)
            {
                Eposta = kullanici.Email;
                Telefon = TelefonuFormatla(kullanici.Telefon);
            }

            try
            {
                var siparisler = _context.Set<Siparis>();
                if (siparisler != null)
                {
                    KullaniciSiparisleri = siparisler.Where(s => s.KullaniciId == userId).ToList();
                }

                KullaniciFavorileri = _context.Set<Favori>()
                    .Where(f => f.KullaniciId == userId)
                    .Take(3)
                    .ToList();

                KullaniciRandevulari = _context.Set<Randevu>()
                    .Where(r => r.KullaniciId == userId)
                    .OrderByDescending(r => r.Id)
                    .ToList();
            }
            catch { }

            return Page();
        }

        public IActionResult OnPost()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToPage("/Auth/Login");
            }

            int userId = int.Parse(userIdStr);

            var kullanici = _context.Set<User>().FirstOrDefault(k => k.Id == userId);
            if (kullanici != null)
            {
                kullanici.Email = Eposta;
                var temizTelefon = TelefonRakamlari(Telefon);
                if (temizTelefon.Length != 11 || !temizTelefon.StartsWith("0"))
                {
                    BilgiMesaji = "Telefon numarası 0 ile başlayan 11 haneli olmalıdır.";
                    KullaniciVerileriniYukle(userId);
                    return Page();
                }

                kullanici.Telefon = TelefonuFormatla(temizTelefon);

                if (!string.IsNullOrEmpty(Sifre))
                {
                    if (Sifre.Length < 6)
                    {
                        BilgiMesaji = "Şifre en az 6 karakter olmalıdır.";
                        KullaniciVerileriniYukle(userId);
                        return Page();
                    }

                    kullanici.Sifre = PasswordHasher.HashPassword(Sifre);
                }

                _context.SaveChanges();
                BilgiMesaji = "🔒 Bilgileriniz başarıyla güncellendi!";
            }

            try
            {
                var siparisler = _context.Set<Siparis>();
                if (siparisler != null)
                {
                    KullaniciSiparisleri = siparisler.Where(s => s.KullaniciId == userId).ToList();
                }

                KullaniciFavorileri = _context.Set<Favori>()
                    .Where(f => f.KullaniciId == userId)
                    .Take(3)
                    .ToList();

                KullaniciRandevulari = _context.Set<Randevu>()
                    .Where(r => r.KullaniciId == userId)
                    .OrderByDescending(r => r.Id)
                    .ToList();
            }
            catch { }

            return Page();
        }

        private void KullaniciVerileriniYukle(int userId)
        {
            KullaniciSiparisleri = _context.Set<Siparis>()
                .Where(s => s.KullaniciId == userId)
                .ToList();

            KullaniciFavorileri = _context.Set<Favori>()
                .Where(f => f.KullaniciId == userId)
                .Take(3)
                .ToList();

            KullaniciRandevulari = _context.Set<Randevu>()
                .Where(r => r.KullaniciId == userId)
                .OrderByDescending(r => r.Id)
                .ToList();
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

        public string DurumMetni(string? durum) => DashboardModel.DurumMetni(durum);
    }
}
