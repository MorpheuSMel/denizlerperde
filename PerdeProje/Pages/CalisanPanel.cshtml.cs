using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PerdeProje.Data;
using PerdeProje.Models;

namespace PerdeProje.Pages
{
    public class CalisanPanelModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CalisanPanelModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string PanelBasligi { get; set; } = "Çalışan Paneli";
        public string Rol { get; set; } = "";
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
                Siparisler = await _context.Siparisler
                    .Where(siparis => siparis.UrunAdi.Contains("Fon"))
                    .OrderByDescending(siparis => siparis.SiparisTarihi)
                    .ToListAsync();
                return Page();
            }

            if (Rol.Equals("TulTerzisi", StringComparison.OrdinalIgnoreCase))
            {
                PanelBasligi = "Tül Perde Terzi Paneli";
                Siparisler = await _context.Siparisler
                    .Where(siparis => siparis.UrunAdi.Contains("Tül") || siparis.UrunAdi.Contains("Tul"))
                    .OrderByDescending(siparis => siparis.SiparisTarihi)
                    .ToListAsync();
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
            var siparis = await _context.Siparisler.FindAsync(id);

            if (siparis != null)
            {
                siparis.Durum = durum;
                await _context.SaveChangesAsync();
            }

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
    }
}
