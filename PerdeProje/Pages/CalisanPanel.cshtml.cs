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
        public bool MontajciMi => Rol.Equals("Montajci", StringComparison.OrdinalIgnoreCase);
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
                Siparisler = await MontajaGidecekSiparisleriGetir();
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

            siparis.Durum = DurumDegeri(durum);
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
                .Where(siparis => !PaketlemeyeGidecekMi(siparis)
                    && !KargoyaGidecekMi(siparis)
                    && !MontajaGidecekMi(siparis)
                    && !DurumIcerir(siparis, "Teslim Edildi"))
                .ToList();
        }

        private async Task<List<Siparis>> PaketlenecekSiparisleriGetir()
        {
            var siparisler = await _context.Siparisler
                .OrderByDescending(siparis => siparis.SiparisTarihi)
                .ToListAsync();

            return siparisler
                .Where(PaketlemeyeGidecekMi)
                .ToList();
        }

        private async Task<List<Siparis>> KargoyaGidecekSiparisleriGetir()
        {
            var siparisler = await _context.Siparisler
                .OrderByDescending(siparis => siparis.SiparisTarihi)
                .ToListAsync();

            return siparisler
                .Where(KargoyaGidecekMi)
                .ToList();
        }

        private async Task<List<Siparis>> MontajaGidecekSiparisleriGetir()
        {
            var siparisler = await _context.Siparisler
                .OrderByDescending(siparis => siparis.SiparisTarihi)
                .ToListAsync();

            return siparisler
                .Where(MontajaGidecekMi)
                .ToList();
        }

        private static string DurumDegeri(string durum)
        {
            var temizDurum = (durum ?? "").Trim();

            if (MetinIcerir(temizDurum, "Terziye"))
            {
                return "Terziye Gönderiliyor";
            }

            if (MetinIcerir(temizDurum, "Dikime"))
            {
                return "Dikime Alındı";
            }

            if (MetinIcerir(temizDurum, "Dikim"))
            {
                return "Dikim Tamamlandı";
            }

            if (MetinIcerir(temizDurum, "Paketlemeye"))
            {
                return "Paketlemeye Hazır";
            }

            if (MetinIcerir(temizDurum, "Paketlendi"))
            {
                return "Paketlendi";
            }

            if (MetinIcerir(temizDurum, "Kargoya Verildi"))
            {
                return "Kargoya Verildi";
            }

            if (MetinIcerir(temizDurum, "Kargoya Teslim"))
            {
                return "Kargoya Teslim";
            }

            if (MetinIcerir(temizDurum, "Montaja"))
            {
                return "Montaja Hazır";
            }

            if (MetinIcerir(temizDurum, "Teslimat Başladı"))
            {
                return "Teslimat Başladı";
            }

            if (MetinIcerir(temizDurum, "Teslim Edildi"))
            {
                return "Teslim Edildi";
            }

            return temizDurum;
        }

        private static bool PaketlemeyeGidecekMi(Siparis siparis)
        {
            return DurumIcerir(siparis, "Paketleme")
                && !DurumIcerir(siparis, "Paketlendi")
                && !DurumIcerir(siparis, "Kargoya");
        }

        private static bool KargoyaGidecekMi(Siparis siparis)
        {
            return (DurumIcerir(siparis, "Paketlendi")
                    || DurumIcerir(siparis, "Kargoya Teslim"))
                && !DurumIcerir(siparis, "Kargoya Verildi");
        }

        private static bool MontajaGidecekMi(Siparis siparis)
        {
            return (DurumIcerir(siparis, "Montaja")
                    || DurumIcerir(siparis, "Teslimat Başladı"))
                && !DurumIcerir(siparis, "Teslim Edildi");
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




