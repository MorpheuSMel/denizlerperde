using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using PerdeProje.Data;
using PerdeProje.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerdeProje.Pages
{
    public class FavorilerimModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public FavorilerimModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kullanıcının favori ürünlerini tutacak dinamik liste
        public List<Favori> FavoriUrunler { get; set; } = new List<Favori>();

        public IActionResult OnGet()
        {
            try
            {
                // Kullanıcı giriş yapmış mı kontrol et
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return RedirectToPage("/Auth/Login");
                }

                if (!int.TryParse(userIdStr, out int userId))
                {
                    return RedirectToPage("/Auth/Login");
                }

                // Veritabanından bu kullanıcıya ait favorileri getir
                var favoriler = _context.Favoriler.Where(f => f.KullaniciId == userId).ToList();

                if (favoriler.Count > 0)
                {
                    // Her favori için ürün bilgisini yükle
                    foreach (var favori in favoriler)
                    {
                        if (favori.UrunId > 0 && _context.Urunler != null)
                        {
                            favori.Urun = _context.Urunler.FirstOrDefault(u => u.Id == favori.UrunId);
                        }
                    }
                }

                FavoriUrunler = favoriler;
            }
            catch (Exception ex)
            {
                // Hata durumunda boş liste dön
                FavoriUrunler = new List<Favori>();
            }

            return Page();
        }
    }
}