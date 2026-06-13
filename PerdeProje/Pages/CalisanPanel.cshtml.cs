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
        public static string GorunenDurum(string? durum) => DurumMetni(durum);

        public async Task<IActionResult> OnGetAsync()
        {
            Rol = HttpContext.Session.GetString("UserRole") ?? "";
            var sessionUserId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrWhiteSpace(sessionUserId))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (int.TryParse(sessionUserId, out var userId))
            {
                var kullaniciRolu = await _context.Users
                    .Where(user => user.Id == userId)
                    .Select(user => user.Rol)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrWhiteSpace(kullaniciRolu) && !Rol.Equals(kullaniciRolu, StringComparison.OrdinalIgnoreCase))
                {
                    Rol = kullaniciRolu;
                    HttpContext.Session.SetString("UserRole", Rol);
                }
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

        public async Task<IActionResult> OnPostSiparisDurumAsync(int id, string durum, string hedef = "")
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

            var rol = await AktifRolAsync();
            hedef = SonrakiHedef(rol, durum, hedef);
            siparis.Durum = DurumKayitDegeri(durum, hedef);
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
                .Where(siparis => (HedefIcerir(siparis, "Terzi") || string.IsNullOrWhiteSpace(HedefDegeri(siparis.Durum)) || DurumIcerir(siparis, "Terziye") || DurumIcerir(siparis, "Dikim") || DurumIcerir(siparis, "Dikime"))
                    && !PaketlemeyeGidecekMi(siparis)
                    && !KargoyaGidecekMi(siparis)
                    && !MontajaGidecekMi(siparis)
                    && !DurumIcerir(siparis, "Teslim Edildi"))
                .ToList();
        }

        private async Task<string> AktifRolAsync()
        {
            var rol = HttpContext.Session.GetString("UserRole") ?? "";
            var sessionUserId = HttpContext.Session.GetString("UserId");

            if (int.TryParse(sessionUserId, out var userId))
            {
                var veritabaniRolu = await _context.Users
                    .Where(user => user.Id == userId)
                    .Select(user => user.Rol)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrWhiteSpace(veritabaniRolu))
                {
                    rol = veritabaniRolu;
                    HttpContext.Session.SetString("UserRole", rol);
                }
            }

            return rol;
        }

        private static string SonrakiHedef(string rol, string durum, string hedef)
        {
            if (!string.IsNullOrWhiteSpace(HedefDegeri(hedef)))
            {
                return hedef;
            }

            if (rol.Equals("Paketlemeci", StringComparison.OrdinalIgnoreCase))
            {
                return "Kargo";
            }

            if (rol.Equals("Kargocu", StringComparison.OrdinalIgnoreCase))
            {
                return "";
            }

            if (rol.Equals("Montajci", StringComparison.OrdinalIgnoreCase))
            {
                return "";
            }

            var temizDurum = DurumDegeri(durum);
            return temizDurum switch
            {
                "Paketlemeye Hazır" => "Paketleme",
                "Paketlendi" or "Kargoya Teslim" => "Kargo",
                "Montaja Hazır" or "Teslimat Başladı" => "Montaj",
                "Terziye Gönderiliyor" or "Dikime Alındı" or "Dikim Tamamlandı" => "Terzi",
                _ => ""
            };
        }

        private async Task<List<Siparis>> PaketlenecekSiparisleriGetir()
        {
            var siparisler = await _context.Siparisler
                .OrderByDescending(siparis => siparis.SiparisTarihi)
                .ToListAsync();

            var hedeflenenSiparisler = siparisler
                .Where(PaketlemeyeGidecekMi)
                .ToList();

            if (hedeflenenSiparisler.Count > 0)
            {
                return hedeflenenSiparisler;
            }

            return siparisler
                .Where(AktifPaketlenebilirSiparisMi)
                .Take(12)
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

        public static string DurumKayitDegeri(string durum, string hedef = "")
        {
            var temizDurum = DurumDegeri(durum);
            var temizHedef = HedefDegeri(hedef);

            if (string.IsNullOrWhiteSpace(temizHedef))
            {
                temizHedef = temizDurum switch
                {
                    "Terziye Gönderiliyor" or "Dikime Alındı" or "Dikim Tamamlandı" => "Terzi",
                    "Paketlemeye Hazır" => "Paketleme",
                    "Paketlendi" or "Kargoya Teslim" => "Kargo",
                    "Montaja Hazır" or "Teslimat Başladı" => "Montaj",
                    _ => ""
                };
            }

            return string.IsNullOrWhiteSpace(temizHedef)
                ? temizDurum
                : $"{temizDurum}||{temizHedef}";
        }

        private static string DurumMetni(string? durum)
        {
            var parcalar = (durum ?? "").Split("||", StringSplitOptions.None);
            return DurumDegeri(parcalar[0]);
        }

        private static string HedefDegeri(string? durum)
        {
            var parcalar = (durum ?? "").Split("||", StringSplitOptions.None);
            var hedef = parcalar.Length > 1 ? parcalar[1].Trim() : (durum ?? "").Trim();

            if (MetinIcerir(hedef, "Terzi"))
            {
                return "Terzi";
            }

            if (MetinIcerir(hedef, "Paket"))
            {
                return "Paketleme";
            }

            if (MetinIcerir(hedef, "Kargo"))
            {
                return "Kargo";
            }

            if (MetinIcerir(hedef, "Montaj"))
            {
                return "Montaj";
            }

            return "";
        }

        private static bool PaketlemeyeGidecekMi(Siparis siparis)
        {
            return (HedefIcerir(siparis, "Paketleme")
                    || DurumIcerir(siparis, "Paketleme")
                    || DurumIcerir(siparis, "Paketlemeye Hazır"))
                && !DurumIcerir(siparis, "Paketlendi")
                && !DurumIcerir(siparis, "Kargoya");
        }

        private static bool AktifPaketlenebilirSiparisMi(Siparis siparis)
        {
            return !DurumIcerir(siparis, "Paketlendi")
                && !DurumIcerir(siparis, "Kargoya Teslim")
                && !DurumIcerir(siparis, "Kargoya Verildi")
                && !DurumIcerir(siparis, "Teslim Edildi")
                && !MontajaGidecekMi(siparis);
        }

        private static bool KargoyaGidecekMi(Siparis siparis)
        {
            if (DurumIcerir(siparis, "Kargoya Verildi"))
            {
                return false;
            }

            return HedefIcerir(siparis, "Kargo")
                || DurumIcerir(siparis, "Paketlendi")
                || DurumIcerir(siparis, "Kargoya Teslim");
        }

        private static bool MontajaGidecekMi(Siparis siparis)
        {
            return (HedefIcerir(siparis, "Montaj")
                    || DurumIcerir(siparis, "Montaja")
                    || DurumIcerir(siparis, "Teslimat Başladı"))
                && !DurumIcerir(siparis, "Teslim Edildi");
        }

        private static bool DurumIcerir(Siparis siparis, string durum)
        {
            return MetinIcerir(DurumMetni(siparis.Durum), durum);
        }

        private static bool HedefIcerir(Siparis siparis, string hedef)
        {
            return HedefDegeri(siparis.Durum) == HedefDegeri(hedef);
        }

        private static bool MetinIcerir(string kaynak, string aranan)
        {
            return kaynak.IndexOf(aranan, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
    }
}




