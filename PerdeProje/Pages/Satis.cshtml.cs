using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PerdeProje.Data;
using PerdeProje.Models;

namespace PerdeProje.Pages
{
    public class SatisModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SatisModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Urun> Urunler { get; set; } = new();
        public List<string> Kategoriler { get; set; } = new();
        public HashSet<int> FavoriUrunIds { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Kategori { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Siralama { get; set; } = "ad-artan";

        [TempData]
        public string? Mesaj { get; set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(Mesaj) &&
                !Mesaj.Contains("sepete", StringComparison.OrdinalIgnoreCase) &&
                !Mesaj.Contains("favori", StringComparison.OrdinalIgnoreCase) &&
                !Mesaj.Contains("urun", StringComparison.OrdinalIgnoreCase) &&
                !Mesaj.Contains("ürün", StringComparison.OrdinalIgnoreCase))
            {
                Mesaj = null;
            }

            await EnsureCatalogAsync();

            Kategoriler = await _context.Urunler
                .Select(urun => urun.Kategori)
                .Distinct()
                .OrderBy(kategori => kategori)
                .ToListAsync();

            IQueryable<Urun> sorgu = _context.Urunler;

            if (!string.IsNullOrWhiteSpace(Kategori))
            {
                sorgu = sorgu.Where(urun => urun.Kategori == Kategori);
            }

            sorgu = Siralama switch
            {
                "fiyat-artan" => sorgu.OrderBy(urun => urun.Fiyat),
                "fiyat-azalan" => sorgu.OrderByDescending(urun => urun.Fiyat),
                "stok-azalan" => sorgu.OrderByDescending(urun => urun.Stok),
                "ad-azalan" => sorgu.OrderByDescending(urun => urun.Ad),
                _ => sorgu.OrderBy(urun => urun.Ad)
            };

            Urunler = await sorgu.ToListAsync();

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrWhiteSpace(userIdStr) && int.TryParse(userIdStr, out var userId))
            {
                FavoriUrunIds = (await _context.Favoriler
                    .Where(favori => favori.KullaniciId == userId)
                    .Select(favori => favori.UrunId)
                    .ToListAsync())
                    .ToHashSet();
            }
        }

        public async Task<IActionResult> OnPostSepeteEkleAsync(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrWhiteSpace(userIdStr))
            {
                return RedirectToPage("/Auth/Login");
            }

            await EnsureCatalogAsync();

            var urun = await _context.Urunler.FindAsync(id);

            if (urun == null)
            {
                Mesaj = "Ürün bulunamadı.";
                return RedirectToPage();
            }

            var sepetIds = HttpContext.Session.GetString("SepetUrunIds");
            var ids = string.IsNullOrWhiteSpace(sepetIds)
                ? new List<int>()
                : sepetIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

            ids.Add(urun.Id);
            HttpContext.Session.SetString("SepetUrunIds", string.Join(",", ids));
            Mesaj = $"{urun.Ad} sepete eklendi. Siparişi sepet sayfasından tamamlayabilirsiniz.";

            return RedirectToPage(new { Kategori, Siralama });
        }

        public async Task<IActionResult> OnPostFavoriyeEkleAsync(int id)
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
                Mesaj = "Ürün bulunamadı.";
                return RedirectToPage();
            }

            var mevcutFavori = await _context.Favoriler
                .FirstOrDefaultAsync(favori => favori.KullaniciId == userId && favori.UrunId == id);

            try
            {
                if (mevcutFavori == null)
                {
                    _context.Favoriler.Add(new Favori
                    {
                        KullaniciId = userId,
                        UrunId = id,
                        UrunAdi = urun.Ad
                    });
                    Mesaj = "Ürün favorilere eklendi.";
                }
                else
                {
                    _context.Favoriler.Remove(mevcutFavori);
                    Mesaj = "Ürün favorilerden kaldırıldı.";
                }

                await _context.SaveChangesAsync();
            }
            catch
            {
                Mesaj = "Favori işlemi yapılırken bir sorun oluştu. Lütfen tekrar deneyin.";
                return RedirectToPage(new { Kategori, Siralama });
            }
            return RedirectToPage(new { Kategori, Siralama });
        }

        private async Task EnsureCatalogAsync()
        {
            var katalog = CatalogProducts();
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
                "Güneşlik Stor Perde",
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
            var eklenecekler = CatalogProducts()
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

        public static List<Urun> CatalogProducts() => new()
        {
            new Urun { Ad = "Dokulu Tül Perde", Aciklama = "Dokulu yüzeyiyle modern ve ferah tül perde.", Fiyat = 950, ResimUrl = "/images/products/custom/dokulu-tul.jpeg", Kategori = "Tül Perde", Stok = 34 },
            new Urun { Ad = "Su Damlası Tül Perde", Aciklama = "Su damlası dokulu, ışığı yumuşak geçiren tül perde.", Fiyat = 1050, ResimUrl = "/images/products/custom/su-damlasi-tul.jpeg", Kategori = "Tül Perde", Stok = 28 },
            new Urun { Ad = "Krem Tül Perde", Aciklama = "Krem tonlu, sade ve aydınlık görünümlü tül perde.", Fiyat = 900, ResimUrl = "/images/products/custom/krem-tul.jpeg", Kategori = "Tül Perde", Stok = 36 },
            new Urun { Ad = "Dokulu Beyaz Tül Perde", Aciklama = "Beyaz dokulu, pileli ve şık tül perde.", Fiyat = 1150, ResimUrl = "/images/products/custom/dokulu-beyaz-tul.jpeg", Kategori = "Tül Perde", Stok = 30 },
            new Urun { Ad = "Ekru Tül Perde", Aciklama = "Ekru renkli, salon ve oturma odası için zarif tül perde.", Fiyat = 1200, ResimUrl = "/images/products/custom/ekru-tul.jpeg", Kategori = "Tül Perde", Stok = 25 },
            new Urun { Ad = "Keten Tül Perde", Aciklama = "Keten dokulu, doğal görünümlü tül perde.", Fiyat = 1350, ResimUrl = "/images/products/custom/keten-tul-perde.jpeg", Kategori = "Tül Perde", Stok = 20 },
            new Urun { Ad = "Dikey Tül Perde", Aciklama = "Dikey formda dekoratif tül perde uygulaması.", Fiyat = 1650, ResimUrl = "/images/products/custom/dikey-tul-perde.jpeg", Kategori = "Tül Perde", Stok = 16 },
            new Urun { Ad = "Kahverengi Tül Fon Perde", Aciklama = "Tül ve fonu bir arada kullanan kahverengi kombin perde.", Fiyat = 2950, ResimUrl = "/images/products/custom/kahverengi-tul-fon.jpeg", IkinciResimUrl = "/images/products/custom/fon-kartela.jpeg", Kategori = "Fon Perde", Stok = 18 },
            new Urun { Ad = "Siyah Fon Perde", Aciklama = "Siyah renkli, tok duruşlu dekoratif fon perde.", Fiyat = 2200, ResimUrl = "/images/products/custom/siyah-fon.jpeg", IkinciResimUrl = "/images/products/custom/fon-kartela.jpeg", Kategori = "Fon Perde", Stok = 22 },
            new Urun { Ad = "Kruvaze Fon Perde", Aciklama = "Kruvaze bağlama detaylı salon fon perdesi.", Fiyat = 3600, ResimUrl = "/images/products/custom/kruvaze-fon-perde.jpeg", IkinciResimUrl = "/images/products/custom/keten-fon-kartela.jpeg", Kategori = "Fon Perde", Stok = 12 },
            new Urun { Ad = "Turuncu Fon Perde", Aciklama = "Canlı turuncu tonuyla dekoratif fon perde.", Fiyat = 2600, ResimUrl = "/images/products/custom/turuncu-fon-perde.jpeg", IkinciResimUrl = "/images/products/custom/keten-fon-kartela.jpeg", Kategori = "Fon Perde", Stok = 17 },
            new Urun { Ad = "Rustik Fon Perde", Aciklama = "Rustik boru uyumlu modern fon perde.", Fiyat = 3100, ResimUrl = "/images/products/custom/rustik-fon.jpeg", IkinciResimUrl = "/images/products/custom/fon-kartela.jpeg", Kategori = "Fon Perde", Stok = 14 },
            new Urun { Ad = "Tül Fon Perde", Aciklama = "Tül ve fon perdeyi birlikte sunan dekoratif uygulama.", Fiyat = 3200, ResimUrl = "/images/products/custom/tul-fon.jpeg", IkinciResimUrl = "/images/products/custom/keten-fon-kartela.jpeg", Kategori = "Fon Perde", Stok = 15 },
            new Urun { Ad = "Gri Tül Fon Perde", Aciklama = "Gri fon ve beyaz tül kombinli perde modeli.", Fiyat = 3400, ResimUrl = "/images/products/custom/gri-tul-fon.jpeg", IkinciResimUrl = "/images/products/custom/fon-kartela.jpeg", Kategori = "Fon Perde", Stok = 13 },
            new Urun { Ad = "Beyaz Zebra Perde", Aciklama = "Gündüz-gece kullanımına uygun beyaz zebra perde.", Fiyat = 1450, ResimUrl = "/images/products/custom/beyaz-zebra.jpeg", Kategori = "Zebra Perde", Stok = 32 },
            new Urun { Ad = "Geçişli Stor Perde", Aciklama = "Geçişli dokulu, mekanizmalı stor perde.", Fiyat = 1550, ResimUrl = "/images/products/custom/gecisli-stor-perde.jpeg", Kategori = "Stor Perde", Stok = 30 },
            new Urun { Ad = "Siyah Stor Perde", Aciklama = "Siyah renkli, modern çizgili stor perde.", Fiyat = 1650, ResimUrl = "/images/products/custom/siyah-stor.jpeg", Kategori = "Stor Perde", Stok = 24 },
            new Urun { Ad = "Ekru Stor Perde", Aciklama = "Ekru tonlu, zincir mekanizmalı stor perde.", Fiyat = 1500, ResimUrl = "/images/products/custom/ekru-stor.jpeg", Kategori = "Stor Perde", Stok = 26 },
            new Urun { Ad = "Blackout Siyah Perde", Aciklama = "Tam karartma sağlayan siyah blackout perde.", Fiyat = 2100, ResimUrl = "/images/products/custom/blackout-siyah.jpeg", Kategori = "Blackout Perde", Stok = 18 },
            new Urun { Ad = "Akıllı Sistem Blackout", Aciklama = "Akıllı sisteme uyumlu blackout perde uygulaması.", Fiyat = 6500, ResimUrl = "/images/products/custom/akilli-sistem-blackout.jpeg", IkinciResimUrl = "/images/products/custom/akilli-sistem-mekanizma.jpeg", Kategori = "Motorlu Perde", Stok = 8 },
            new Urun { Ad = "Akıllı Sistem Geçişli Tül", Aciklama = "Motorlu sistemle çalışan geçişli tül perde.", Fiyat = 7200, ResimUrl = "/images/products/custom/akilli-sistem-gecisli-tul.jpeg", IkinciResimUrl = "/images/products/custom/akilli-sistem-mekanizma.jpeg", Kategori = "Motorlu Perde", Stok = 7 },
            new Urun { Ad = "Akıllı Sistem Mekanizma", Aciklama = "Perde motoru ve akıllı mekanizma.", Fiyat = 3800, ResimUrl = "/images/products/custom/akilli-sistem-mekanizma.jpeg", Kategori = "Motorlu Perde", Stok = 10 },
            new Urun { Ad = "Akıllı Dikey Jaluzi", Aciklama = "Dikey jaluzi perde için akıllı sistem uygulaması.", Fiyat = 5900, ResimUrl = "/images/products/custom/akilli-dikey-jaluzi.jpeg", IkinciResimUrl = "/images/products/custom/akilli-sistem-mekanizma.jpeg", Kategori = "Jaluzi Perde", Stok = 9 },
            new Urun { Ad = "Gri Jaluzi Perde", Aciklama = "Gri tonlu, ayarlanabilir lamelli jaluzi perde.", Fiyat = 2400, ResimUrl = "/images/products/custom/gri-jaluzi-perde.jpeg", Kategori = "Jaluzi Perde", Stok = 18 },
            new Urun { Ad = "Ahşap Jaluzi Perde", Aciklama = "Ahşap görünümlü, sıcak tonlu jaluzi perde.", Fiyat = 3200, ResimUrl = "/images/products/custom/ahsap-jaluzi.jpeg", Kategori = "Jaluzi Perde", Stok = 15 }
        };
    }
}

