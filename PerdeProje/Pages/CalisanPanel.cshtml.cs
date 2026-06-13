using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PerdeProje.Data;
using PerdeProje.Models;

namespace PerdeProje.Pages
{
    [IgnoreAntiforgeryToken]
    public class CalisanPanelModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CalisanPanelModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string PanelBasligi { get; set; } = "Çalışan Paneli";
        public string Rol { get; set; } = "";
        public bool AkilliSistemciMi => Rol.Equals("AkilliSistemci", StringComparison.OrdinalIgnoreCase);
        public bool PaketlemeciMi => Rol.Equals("Paketlemeci", StringComparison.OrdinalIgnoreCase);
        public bool KargocuMu => Rol.Equals("Kargocu", StringComparison.OrdinalIgnoreCase);
        public List<Siparis> Siparisler { get; set; } = new();
        public List<Randevu> Randevular { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Rol = HttpContext.Session.GetString("UserRole") ?? "";

            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (Rol.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Admin/Dashboard");
            }

            if (Rol.Equals("FonTerzisi", StringComparison.OrdinalIgnoreCase))
            {
                PanelBasligi = "Fon Perde Terzi Paneli";
                Siparisler = await SiparisleriGetir("Fon");
                return Page();
            }

            if (Rol.Equals("TulTerzisi", StringComparison.OrdinalIgnoreCase))
            {
                PanelBasligi = "Tül Perde Terzi Paneli";
                Siparisler = await SiparisleriGetir("Tül", "Tul");
                return Page();
            }

            if (Rol.Equals("AkilliSistemci", StringComparison.OrdinalIgnoreCase))
            {
                PanelBasligi = "Akıllı Sistem Usta Paneli";
                Siparisler = await SiparisleriGetir("Akıllı", "Akilli", "Motorlu", "Sistem");
                return Page();
            }

            if (Rol.Equals("Paketlemeci", StringComparison.OrdinalIgnoreCase))
            {
                PanelBasligi = "Paketleme Personeli Paneli";
                Siparisler = await PaketlenecekSiparisleriGetir();
                return Page();
            }

            if (Rol.Equals("Kargocu", StringComparison.OrdinalIgnoreCase))
            {
                PanelBasligi = "Kargo Personeli Paneli";
                Siparisler = await KargoyaGidecekSiparisleriGetir();
                return Page();
            }

            if (Rol.Equals("Montajci", StringComparison.OrdinalIgnoreCase))
            {
                PanelBasligi = "Montajcı Paneli";
                Randevular = await _context.Randevular
                    .OrderByDescending(randevu => randevu.Id)
                    .ToListAsync();
                return Page();
            }

            return RedirectToPage("/KullaniciSayfasi");
        }

        public async Task<IActionResult> OnPostSiparisDurumAsync(int id, string durum)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(durum))
            {
                return RedirectToPage();
            }

            var siparis = await _context.Siparisler.FindAsync(id);
            if (siparis == null)
            {
                return RedirectToPage();
            }

            siparis.Durum = durum.Trim();
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRandevuDurumAsync(int id, string durum)
        {
            var randevu = await _context.Randevular.FindAsync(id);

            if (randevu != null)
            {
                randevu.DurumEtiketi = durum;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        private async Task<List<Siparis>> SiparisleriGetir(params string[] anahtarlar)
        {
            var siparisler = await _context.Siparisler
                .OrderByDescending(siparis => siparis.SiparisTarihi)
                .ToListAsync();

            return siparisler
                .Where(siparis => anahtarlar.Any(anahtar => MetinIcerir(siparis.UrunAdi, anahtar)))
                .Where(siparis => !DurumIcerir(siparis, "Paketlemeye Hazır")
                    && !DurumIcerir(siparis, "Paketlendi")
                    && !DurumIcerir(siparis, "Kargoya Verildi"))
                .ToList();
        }

        private Task<List<Siparis>> PaketlenecekSiparisleriGetir()
        {
            return _context.Siparisler
                .Where(siparis => (siparis.Durum ?? "").Contains("Paketlemeye Hazır"))
                .OrderByDescending(siparis => siparis.SiparisTarihi)
                .ToListAsync();
        }

        private Task<List<Siparis>> KargoyaGidecekSiparisleriGetir()
        {
            return _context.Siparisler
                .Where(siparis => (siparis.Durum ?? "").Contains("Paketlendi"))
                .OrderByDescending(siparis => siparis.SiparisTarihi)
                .ToListAsync();
        }

        private static bool DurumIcerir(Siparis siparis, string durum)
        {
            return MetinIcerir(siparis.Durum ?? "", durum);
        }

        private static bool MetinIcerir(string kaynak, string aranan)
        {
            return kaynak.IndexOf(aranan, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
    }
}




