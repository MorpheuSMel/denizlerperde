using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PerdeProje.Data;
using PerdeProje.Models;

namespace PerdeProje.Pages
{
    public class SepetModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SepetModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Urun> Urunler { get; set; } = new();
        public decimal ToplamTutar => Urunler.Sum(urun => urun.Fiyat);

        [TempData]
        public string? Mesaj { get; set; }

        [BindProperty]
        public string TeslimatAdresi { get; set; } = "";

        [BindProperty]
        public string KargoNotu { get; set; } = "";

        [BindProperty]
        public string KartUzerindekiIsim { get; set; } = "";

        [BindProperty]
        public string KartNumarasi { get; set; } = "";

        [BindProperty]
        public string SonKullanmaTarihi { get; set; } = "";

        [BindProperty]
        public string Cvv { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToPage("/Auth/Login");
            }

            Urunler = await SepetUrunleriniGetirAsync();
            return Page();
        }

        public IActionResult OnPostTemizle()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToPage("/Auth/Login");
            }

            HttpContext.Session.Remove("SepetUrunIds");
            Mesaj = "Sepet temizlendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSiparisOlusturAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var urunler = await SepetUrunleriniGetirAsync();

            if (urunler.Count == 0)
            {
                Mesaj = "Sepetiniz boş.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(TeslimatAdresi) ||
                string.IsNullOrWhiteSpace(KartUzerindekiIsim) ||
                string.IsNullOrWhiteSpace(KartNumarasi) ||
                string.IsNullOrWhiteSpace(SonKullanmaTarihi) ||
                string.IsNullOrWhiteSpace(Cvv))
            {
                Urunler = urunler;
                Mesaj = "Siparişi onaylamak için teslimat ve kart bilgilerini doldurun.";
                return Page();
            }

            var kartRakamlari = SadeceRakam(KartNumarasi);
            var sktRakamlari = SadeceRakam(SonKullanmaTarihi);
            var cvvRakamlari = SadeceRakam(Cvv);

            if (kartRakamlari.Length != 16 || sktRakamlari.Length != 4 || cvvRakamlari.Length != 3)
            {
                Urunler = urunler;
                Mesaj = "Kart numarası 16 hane, son kullanma tarihi AA/YY, CVV 3 hane olmalıdır.";
                return Page();
            }

            var siparisKodlari = new List<string>();

            try
            {
                foreach (var urun in urunler)
                {
                    var siparisKodu = "DP-" + DateTime.Now.ToString("yyMMddHHmmss") + "-" + urun.Id;
                    siparisKodlari.Add(siparisKodu);

                    _context.Siparisler.Add(new Siparis
                    {
                        KullaniciId = userId,
                        SiparisKodu = siparisKodu,
                        UrunAdi = urun.Ad,
                        SiparisTarihi = DateTime.Now,
                        Durum = "Sipariş Alındı"
                    });
                }

                await _context.SaveChangesAsync();
            }
            catch
            {
                Urunler = urunler;
                Mesaj = "Sipariş oluşturulamadı. Lütfen tekrar deneyin.";
                return Page();
            }

            HttpContext.Session.Remove("SepetUrunIds");
            TempData["PanelMesaji"] = "Sipari\u015Finiz al\u0131nd\u0131: " + string.Join(", ", siparisKodlari);
            return RedirectToPage("/KullaniciSayfasi");
        }

        private static string SadeceRakam(string? deger)
        {
            return new string((deger ?? string.Empty).Where(char.IsDigit).ToArray());
        }

        private async Task<List<Urun>> SepetUrunleriniGetirAsync()
        {
            var sepetIds = HttpContext.Session.GetString("SepetUrunIds");

            if (string.IsNullOrWhiteSpace(sepetIds))
            {
                return new List<Urun>();
            }

            var ids = sepetIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out var urunId) ? urunId : 0)
                .Where(id => id > 0)
                .ToList();

            if (ids.Count == 0)
            {
                return new List<Urun>();
            }

            var urunler = await _context.Urunler
                .Where(urun => ids.Contains(urun.Id))
                .ToListAsync();

            return ids
                .Select(id => urunler.FirstOrDefault(urun => urun.Id == id))
                .Where(urun => urun != null)
                .Cast<Urun>()
                .ToList();
        }
    }
}
