using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using PerdeProje.Data;
using PerdeProje.Models;
using PerdeProje.Services;
using System.Linq;
using System.Collections.Generic;

namespace PerdeProje.Pages
{
    public class HesapAyarlariModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public HesapAyarlariModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty] public string Ad { get; set; } = string.Empty;
        [BindProperty] public string Soyad { get; set; } = string.Empty;
        [BindProperty] public string Eposta { get; set; } = string.Empty;
        [BindProperty] public string Telefon { get; set; } = string.Empty;
        [BindProperty] public string? YeniSifre { get; set; }

        // Yeni Giriş Alanları İçin Bound Property'ler
        [BindProperty] public string YeniAdresBaslik { get; set; } = string.Empty;
        [BindProperty] public string YeniAcikAdres { get; set; } = string.Empty;
        [BindProperty] public string YeniKartBasligi { get; set; } = string.Empty;
        [BindProperty] public string YeniKartUzerindekiIsim { get; set; } = string.Empty;
        [BindProperty] public string YeniKartNumarasi { get; set; } = string.Empty;
        [BindProperty] public string YeniSonKullanmaTarihi { get; set; } = string.Empty;
        [BindProperty] public string YeniCvv { get; set; } = string.Empty;

        public List<Adres> KullaniciAdresleri { get; set; } = new();
        public List<Kart> KullaniciKartlari { get; set; } = new();
        public string BilgiMesaji { get; set; } = string.Empty;

        private void KullaniciVerileriniYukle(int userId)
        {
            var kullanici = _context.Set<User>().FirstOrDefault(k => k.Id == userId);
            if (kullanici != null)
            {
                Ad = kullanici.Ad;
                Soyad = kullanici.Soyad;
                Eposta = kullanici.Email;
                Telefon = TelefonuFormatla(kullanici.Telefon);
            }

            // Hata veren listeleme satırları güvenli hale getirildi
            KullaniciAdresleri = _context.Set<Adres>().Where(a => a.UserId == userId).ToList();
            KullaniciKartlari = _context.Set<Kart>().Where(k => k.UserId == userId).ToList();
        }

        public IActionResult OnGet()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Auth/Login");

            KullaniciVerileriniYukle(int.Parse(userIdStr));
            return Page();
        }

        public IActionResult OnPostProfilGuncelle()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Auth/Login");

            int userId = int.Parse(userIdStr);
            var kullanici = _context.Set<User>().FirstOrDefault(k => k.Id == userId);

            if (kullanici != null)
            {
                kullanici.Ad = Ad;
                kullanici.Soyad = Soyad;
                kullanici.Email = Eposta;
                var temizTelefon = TelefonRakamlari(Telefon);
                if (temizTelefon.Length != 11 || !temizTelefon.StartsWith("0"))
                {
                    BilgiMesaji = "Telefon numarasi 0 ile baslayan 11 haneli olmalidir.";
                    KullaniciVerileriniYukle(userId);
                    return Page();
                }

                kullanici.Telefon = TelefonuFormatla(temizTelefon);

                if (!string.IsNullOrEmpty(YeniSifre))
                {
                    if (YeniSifre.Length < 6)
                    {
                        BilgiMesaji = "Şifre en az 6 karakter olmalıdır.";
                        KullaniciVerileriniYukle(userId);
                        return Page();
                    }

                    if (PasswordHasher.VerifyPassword(YeniSifre, kullanici.Sifre) || kullanici.Sifre == YeniSifre)
                    {
                        BilgiMesaji = "Yeni şifre mevcut şifrenizle aynı olamaz.";
                        KullaniciVerileriniYukle(userId);
                        return Page();
                    }

                    kullanici.Sifre = PasswordHasher.HashPassword(YeniSifre);
                }

                _context.SaveChanges();
                BilgiMesaji = "✨ Profil ve güvenlik bilgileriniz başarıyla güncellendi!";
            }

            KullaniciVerileriniYukle(userId);
            return Page();
        }

        public IActionResult OnPostAdresEkle()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Auth/Login");

            int userId = int.Parse(userIdStr);

            if (!string.IsNullOrEmpty(YeniAdresBaslik) && !string.IsNullOrEmpty(YeniAcikAdres))
            {
                var yeniAdres = new Adres
                {
                    Baslik = YeniAdresBaslik,
                    AcikAdres = YeniAcikAdres,
                    UserId = userId
                };
                _context.Set<Adres>().Add(yeniAdres);
                _context.SaveChanges();
                BilgiMesaji = "🏠 Yeni adresiniz başarıyla kaydedildi!";
            }

            KullaniciVerileriniYukle(userId);
            return Page();
        }

        public IActionResult OnPostKartEkle()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToPage("/Auth/Login");

            int userId = int.Parse(userIdStr);

            if (!string.IsNullOrEmpty(YeniKartBasligi) && !string.IsNullOrEmpty(YeniKartNumarasi))
            {
                var yeniKart = new Kart
                {
                    KartBasligi = YeniKartBasligi,
                    KartUzerindekiIsim = YeniKartUzerindekiIsim,
                    KartNumarasi = YeniKartNumarasi,
                    SonKullanmaTarihi = YeniSonKullanmaTarihi,
                    Cvv = YeniCvv,
                    UserId = userId
                };
                _context.Set<Kart>().Add(yeniKart);
                _context.SaveChanges();
                BilgiMesaji = "💳 Yeni ödeme kartınız başarıyla kaydedildi!";
            }

            KullaniciVerileriniYukle(userId);
            return Page();
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
