using Microsoft.AspNetCore.Mvc.RazorPages;
using PerdeProje.Data;
using PerdeProje.Models;
using System.Collections.Generic;
using System.Linq;

namespace PerdeProje.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ön yüzde listelemek için ürün listesi oluşturuyoruz
        public List<Urun> HazirPerdeler { get; set; } = new();
        // Örnek olarak buradaki listenin ismi ne diye geçiyor?
        public List<Urun> Urunler { get; set; } // Veya public List<Urun> Urun { get; set; }

        public void OnGet()
        {
            // Veritabanındaki tüm hazır perdeleri listele
            HazirPerdeler = _context.Urunler.ToList();
        }
    }
}