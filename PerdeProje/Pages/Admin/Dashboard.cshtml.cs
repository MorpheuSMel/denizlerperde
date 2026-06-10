using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PerdeProje.Data;
using PerdeProje.Models;

namespace PerdeProje.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        private const string StageRandevu = "randevu";
        private const string StageKesim = "kesim";
        private const string StageTerzi = "terzi";
        private const string StageKargo = "kargo";
        private const string StageSaha = "saha";

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<SiparisKutusu> YeniRandevular { get; set; } = new();
        public List<SiparisKutusu> KesimBekleyenler { get; set; } = new();
        public List<SiparisKutusu> TerziDikimindekiler { get; set; } = new();
        public List<SiparisKutusu> KargoBekleyenler { get; set; } = new();
        public List<SiparisKutusu> SahaMontajdakiler { get; set; } = new();
        public List<StokKutusu> KritikKumaslar { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await VerileriDoldurAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetUpdateRandevuStatusAsync(int id, string newStatus)
        {
            var randevu = await _context.Randevular.FindAsync(id);

            if (randevu == null)
            {
                return NotFound();
            }

            randevu.DurumEtiketi = "Ölçü Alındı";
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, stage = StageKesim, status = DurumMetni(randevu.DurumEtiketi) });
        }

        public async Task<IActionResult> OnGetUpdateSiparisStatusAsync(int id, string newStatus)
        {
            var siparis = await _context.Siparisler.FindAsync(id);

            if (siparis == null)
            {
                return NotFound();
            }

            siparis.Durum = DurumMetni(newStatus);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, status = DurumMetni(siparis.Durum) });
        }

        public Task<IActionResult> OnPostUpdateRandevuStatusAsync(int id, string newStatus) => OnGetUpdateRandevuStatusAsync(id, newStatus);
        public Task<IActionResult> OnPostUpdateSiparisStatusAsync(int id, string newStatus) => OnGetUpdateSiparisStatusAsync(id, newStatus);

        private async Task VerileriDoldurAsync()
        {
            var randevular = await _context.Randevular
                .OrderByDescending(randevu => randevu.Id)
                .ToListAsync();

            YeniRandevular = randevular
                .Where(randevu => !DurumMetni(randevu.DurumEtiketi).Contains("Ölçü", StringComparison.OrdinalIgnoreCase))
                .Select(RandevuKutusu)
                .ToList();

            KesimBekleyenler = randevular
                .Where(randevu => DurumMetni(randevu.DurumEtiketi).Contains("Ölçü", StringComparison.OrdinalIgnoreCase))
                .Select(RandevuKutusu)
                .ToList();

            var siparisler = await _context.Siparisler
                .OrderByDescending(siparis => siparis.SiparisTarihi)
                .ToListAsync();

            var kullanicilar = await _context.Users.ToDictionaryAsync(user => user.Id);

            TerziDikimindekiler = siparisler
                .Where(siparis => TerziAsamasindaMi(siparis.Durum)
                    || siparis.Durum.Contains("Siparis", StringComparison.OrdinalIgnoreCase)
                    || siparis.Durum.Contains("Sipariş", StringComparison.OrdinalIgnoreCase)
                    || siparis.Durum.Contains("Alindi", StringComparison.OrdinalIgnoreCase)
                    || siparis.Durum.Contains("Alındı", StringComparison.OrdinalIgnoreCase)
                    || siparis.Durum.Contains("Terzi", StringComparison.OrdinalIgnoreCase)
                    || siparis.Durum.Contains("Dikil", StringComparison.OrdinalIgnoreCase)
                    || siparis.Durum.Contains("Atolye", StringComparison.OrdinalIgnoreCase))
                .Select(siparis => SiparisKutusu(siparis, StageTerzi, kullanicilar))
                .ToList();

            KargoBekleyenler = siparisler
                .Where(siparis => siparis.Durum.Contains("Kargo", StringComparison.OrdinalIgnoreCase))
                .Select(siparis => SiparisKutusu(siparis, StageKargo, kullanicilar))
                .ToList();

            SahaMontajdakiler = siparisler
                .Where(siparis => siparis.Durum.Contains("Teslim", StringComparison.OrdinalIgnoreCase)
                    || siparis.Durum.Contains("Montaj", StringComparison.OrdinalIgnoreCase))
                .Select(siparis => SiparisKutusu(siparis, StageSaha, kullanicilar))
                .ToList();

            KritikKumaslar = await _context.Urunler
                .Where(urun => urun.Stok <= 20)
                .OrderBy(urun => urun.Stok)
                .Select(urun => new StokKutusu
                {
                    UrunAdi = urun.Ad,
                    UrunKodu = "URN-" + urun.Id.ToString("D3"),
                    SeriNo = urun.Id.ToString("D4"),
                    StokMiktar = urun.Stok
                })
                .ToListAsync();
        }

        private static SiparisKutusu RandevuKutusu(Randevu randevu)
        {
            return new SiparisKutusu
            {
                Id = randevu.Id,
                MusteriAdi = randevu.MusteriAdi,
                PerdeTuru = randevu.PerdeTuru,
                Olcu = randevu.Olcu,
                PileTipi = randevu.PileTipi,
                DurumEtiketi = DurumMetni(randevu.DurumEtiketi),
                Asama = StageRandevu
            };
        }

        private static SiparisKutusu SiparisKutusu(Siparis siparis, string asama, Dictionary<int, User> kullanicilar)
        {
            var musteriAdi = kullanicilar.TryGetValue(siparis.KullaniciId, out var kullanici)
                ? $"{kullanici.Ad} {kullanici.Soyad}".Trim()
                : "Kullanıcı #" + siparis.KullaniciId;

            return new SiparisKutusu
            {
                Id = siparis.Id,
                MusteriAdi = string.IsNullOrWhiteSpace(musteriAdi) ? "Kullanıcı #" + siparis.KullaniciId : musteriAdi,
                PerdeTuru = siparis.UrunAdi,
                Olcu = siparis.SiparisKodu,
                PileTipi = siparis.SiparisTarihi.ToString("dd.MM.yyyy HH:mm"),
                DurumEtiketi = DurumMetni(siparis.Durum),
                Asama = asama
            };
        }

        public static string DurumMetni(string? durum)
        {
            return (durum ?? string.Empty).Trim() switch
            {
                "Siparis alindi" or "Siparis Alindi" or "Sipariş alındı" or "Sipariş alindi" or "SipariÅŸ alÄ±ndÄ±" or "SipariÅŸ AlÄ±ndÄ±" => "Sipariş Alındı",
                "Olcu alindi" or "Olcu Alindi" or "Ölçü alındı" or "Ã–lÃ§Ã¼ AlÄ±ndÄ±" => "Ölçü Alındı",
                "Terziye Gonderiliyor" or "Terziye GÃ¶nderiliyor" => "Terziye Gönderiliyor",
                "Atolye" or "Atolyede" or "Atölyede" => "Atölyede",
                "Kargoya teslim" => "Kargoya Teslim",
                "Kargoya verildi" => "Kargoya Verildi",
                "Teslim edildi" => "Teslim Edildi",
                var temiz => temiz
            };
        }

        private static bool TerziAsamasindaMi(string? durum)
        {
            var temizDurum = DurumMetni(durum);

            return temizDurum.Contains("Sipariş", StringComparison.OrdinalIgnoreCase)
                || temizDurum.Contains("Alındı", StringComparison.OrdinalIgnoreCase)
                || temizDurum.Contains("Terzi", StringComparison.OrdinalIgnoreCase)
                || temizDurum.Contains("Dikil", StringComparison.OrdinalIgnoreCase)
                || temizDurum.Contains("Atölye", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class SiparisKutusu
    {
        public int Id { get; set; }
        public string MusteriAdi { get; set; } = "";
        public string PerdeTuru { get; set; } = "";
        public string Olcu { get; set; } = "";
        public string PileTipi { get; set; } = "";
        public double KumasMetraji { get; set; }
        public string DurumEtiketi { get; set; } = "";
        public string Asama { get; set; } = "";
    }

    public class StokKutusu
    {
        public string UrunAdi { get; set; } = "";
        public string UrunKodu { get; set; } = "";
        public string SeriNo { get; set; } = "";
        public double StokMiktar { get; set; }
    }
}
