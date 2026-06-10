using Microsoft.AspNetCore.Mvc;
using PerdeProje.Data;
using PerdeProje.Models;

namespace PerdeProje.Controllers
{
    public class IletisimController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IletisimController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detaylar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitAppointment(string isim, string telefon, string email, string randevuTarihi, string adres, string model, string mesaj)
        {
            var tarihSaat = DateTime.TryParse(randevuTarihi, out var tarih)
                ? tarih.ToString("dd.MM.yyyy HH:mm")
                : "Tarih bekleniyor";

            _context.Randevular.Add(new Randevu
            {
                KullaniciId = int.TryParse(HttpContext.Session.GetString("UserId"), out var userId) ? userId : null,
                MusteriAdi = isim,
                PerdeTuru = string.IsNullOrWhiteSpace(model) ? "Model belirtilmedi" : model,
                Olcu = tarihSaat,
                PileTipi = string.IsNullOrWhiteSpace(adres) ? mesaj ?? "" : adres,
                DurumEtiketi = "Yeni Talep"
            });
            _context.SaveChanges();

            TempData["RandevuIsim"] = isim;
            TempData["RandevuTelefon"] = telefon;
            TempData["RandevuModel"] = model;
            TempData["RandevuTarihi"] = randevuTarihi;
            return RedirectToAction("Thanks");
        }

        public IActionResult Thanks()
        {
            return View();
        }
    }
}
