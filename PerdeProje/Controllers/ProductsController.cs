using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerdeProje.Data;
using PerdeProje.Models;
using PerdeProje.Pages;

namespace PerdeProje.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            await EnsureCatalogAsync();

            var urunler = await _context.Urunler
                .OrderBy(urun => urun.Kategori)
                .ThenBy(urun => urun.Ad)
                .ToListAsync();

            return View(urunler);
        }

        public async Task<IActionResult> Details(int id)
        {
            await EnsureCatalogAsync();

            var urun = await _context.Urunler.FirstOrDefaultAsync(u => u.Id == id);

            if (urun == null)
            {
                TempData["Mesaj"] = "Ürün bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            return View(urun);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SepeteEkle(int id)
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToPage("/Auth/Login");
            }

            await EnsureCatalogAsync();

            var urun = await _context.Urunler.FindAsync(id);
            if (urun == null)
            {
                TempData["Mesaj"] = "Ürün bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var sepetIds = HttpContext.Session.GetString("SepetUrunIds");
            var ids = string.IsNullOrWhiteSpace(sepetIds)
                ? new List<int>()
                : sepetIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => int.TryParse(value, out var urunId) ? urunId : 0)
                    .Where(urunId => urunId > 0)
                    .ToList();

            ids.Add(urun.Id);
            HttpContext.Session.SetString("SepetUrunIds", string.Join(",", ids));
            TempData["Mesaj"] = $"{urun.Ad} sepete eklendi. Siparişi sepet sayfasından tamamlayabilirsiniz.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FavoriyeEkle(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            await EnsureCatalogAsync();

            var urun = await _context.Urunler.FindAsync(id);
            if (urun == null)
            {
                TempData["Mesaj"] = "Ürün bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var zatenVar = await _context.Favoriler
                .AnyAsync(favori => favori.KullaniciId == userId && favori.UrunId == id);

            if (!zatenVar)
            {
                _context.Favoriler.Add(new Favori
                {
                    KullaniciId = userId,
                    UrunId = id,
                    UrunAdi = urun.Ad
                });
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Mesaj"] = "Ürün favorilere eklendi.";
                }
                catch
                {
                    TempData["Mesaj"] = "Favorilere eklenirken bir sorun oluştu. Lütfen tekrar deneyin.";
                }
            }
            else
            {
                TempData["Mesaj"] = "Bu ürün zaten favorilerinizde.";
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Detaylar()
        {
            return View();
        }

        private async Task EnsureCatalogAsync()
        {
            var katalog = SatisModel.CatalogProducts();
            var eskiKatalogAdlari = new HashSet<string>
            {
                "Dokulu Tul Perde",
                "Su Damlasi Tul Perde",
                "Krem Tul Perde",
                "Dokulu Beyaz Tul Perde",
                "Ekru Tul Perde",
                "Keten Tul Perde",
                "Dikey Tul Perde",
                "Kahverengi Tul Fon Perde",
                "Tul Fon Perde",
                "Gri Tul Fon Perde",
                "Gecisli Stor Perde",
                "Akilli Sistem Blackout",
                "Akilli Sistem Gecisli Tul",
                "Akilli Sistem Mekanizma",
                "Akilli Dikey Jaluzi",
                "Ahsap Jaluzi Perde",
                "Fon Kartela",
                "Keten Fon Kartela",
                "Motorlu Akilli Perde",
                "Zebra Perde Beyaz",
                "Zebra Perde Krem",
                "Guneslik Stor Perde",
                "Blackout Stor Perde",
                "Gipurlu Tul Perde",
                "Duz Ekru Tul Perde",
                "Gipürlü Örme Tül Perde",
                "Güneslik Stor Perde",
                "Güneşlik Stor Perde",
                "Keten Fon Perde",
                "Kadife Fon Perde"
            };
            var mevcutUrunler = await _context.Urunler.ToListAsync();
            var silinecekUrunler = mevcutUrunler
                .Where(urun => eskiKatalogAdlari.Contains(urun.Ad))
                .ToList();
            if (silinecekUrunler.Count > 0)
            {
                var silinecekIds = silinecekUrunler.Select(urun => urun.Id).ToList();
                var eskiFavoriler = await _context.Favoriler
                    .Where(favori => silinecekIds.Contains(favori.UrunId))
                    .ToListAsync();

                if (eskiFavoriler.Count > 0)
                {
                    _context.Favoriler.RemoveRange(eskiFavoriler);
                }

                _context.Urunler.RemoveRange(silinecekUrunler);
                mevcutUrunler = mevcutUrunler.Except(silinecekUrunler).ToList();
            }

            var mevcutAdlar = mevcutUrunler.Select(urun => urun.Ad).ToList();
            var eklenecekler = katalog
                .Where(urun => !mevcutAdlar.Contains(urun.Ad))
                .ToList();

            foreach (var mevcut in mevcutUrunler)
            {
                var katalogUrunu = katalog.FirstOrDefault(urun => urun.Ad == mevcut.Ad);
                if (katalogUrunu == null)
                {
                    continue;
                }

                mevcut.Aciklama = katalogUrunu.Aciklama;
                mevcut.Fiyat = katalogUrunu.Fiyat;
                mevcut.ResimUrl = katalogUrunu.ResimUrl;
                mevcut.IkinciResimUrl = katalogUrunu.IkinciResimUrl;
                mevcut.Kategori = katalogUrunu.Kategori;
                mevcut.Stok = katalogUrunu.Stok;
            }

            if (eklenecekler.Count == 0)
            {
                await _context.SaveChangesAsync();
                return;
            }

            await _context.Urunler.AddRangeAsync(eklenecekler);
            await _context.SaveChangesAsync();
        }
    }
}
