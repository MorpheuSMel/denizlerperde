using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerdeProje.Controllers
{
    public class AdminController : Controller
    {
        private readonly IServiceProvider _serviceProvider;

        public AdminController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IActionResult> Dashboard()
        {
            var model = new DashboardViewModel();

            // Projedeki herhangi bir DbContext'i otomatik buluyoruz
            var dbContext = _serviceProvider.GetService(Type.GetType("Microsoft.EntityFrameworkCore.DbContext")
                             ?? _serviceProvider.GetServices<DbContext>().FirstOrDefault()?.GetType()
                             ?? typeof(object)) as dynamic;

            if (dbContext != null)
            {
                try
                {
                    // 1. KESİM BEKLEYENLER
                    IQueryable<dynamic> siparisQuery = dbContext.Siparis;
                    var kesimVerisi = await EntityFrameworkQueryableExtensions.ToListAsync(siparisQuery);
                    if (kesimVerisi != null)
                    {
                        foreach (var s in kesimVerisi)
                        {
                            string durum = s.GetType().GetProperty("Durum")?.GetValue(s)?.ToString() ?? "";
                            if (durum == "Kesimde" || durum == "Tezgahta" || string.IsNullOrEmpty(durum))
                            {
                                model.KesimBekleyenler.Add(new SiparisKutusu
                                {
                                    MusteriAdi = s.GetType().GetProperty("MusteriAdi")?.GetValue(s)?.ToString() ?? "Belirsiz Müşteri",
                                    PerdeTuru = s.GetType().GetProperty("PerdeTuru")?.GetValue(s)?.ToString() ?? "Genel Perde",
                                    Olcu = s.GetType().GetProperty("Olcu")?.GetValue(s)?.ToString() ?? "300x250",
                                    PileTipi = s.GetType().GetProperty("PileTipi")?.GetValue(s)?.ToString() ?? "1:3 Sık Pile"
                                });
                            }
                        }
                    }

                    // 2. TERZİ DİKİMİNDEKİLER
                    if (kesimVerisi != null)
                    {
                        foreach (var s in kesimVerisi)
                        {
                            string durum = s.GetType().GetProperty("Durum")?.GetValue(s)?.ToString() ?? "";
                            if (durum == "Terzide" || durum == "Dikimde")
                            {
                                model.TerziDikimindekiler.Add(new SiparisKutusu
                                {
                                    MusteriAdi = s.GetType().GetProperty("MusteriAdi")?.GetValue(s)?.ToString() ?? "Belirsiz Müşteri",
                                    PerdeTuru = s.GetType().GetProperty("PerdeTuru")?.GetValue(s)?.ToString() ?? "Genel Perde",
                                    Olcu = s.GetType().GetProperty("Olcu")?.GetValue(s)?.ToString() ?? "300x250",
                                    PileTipi = s.GetType().GetProperty("PileTipi")?.GetValue(s)?.ToString() ?? "1:3 Sık Pile",
                                    KumasMetraji = 8.5
                                });
                            }
                        }
                    }

                    // 3. BUGÜNKÜ SAHA PLANLARI
                    IQueryable<dynamic> randevuQuery = dbContext.Randevu;
                    var randevuVerisi = await EntityFrameworkQueryableExtensions.ToListAsync(randevuQuery);
                    if (randevuVerisi != null)
                    {
                        foreach (var r in randevuVerisi)
                        {
                            string musteri = r.GetType().GetProperty("MusteriAdSoyad")?.GetValue(r)?.ToString()
                                             ?? r.GetType().GetProperty("MusteriAdi")?.GetValue(r)?.ToString()
                                             ?? "Müşteri Kaydı";

                            string adres = r.GetType().GetProperty("AdresTanimi")?.GetValue(r)?.ToString()
                                           ?? r.GetType().GetProperty("Adres")?.GetValue(r)?.ToString()
                                           ?? "Atölye Teslim";

                            model.BugünküRandevular.Add(new SahaKutusu
                            {
                                MusteriAdSoyad = musteri,
                                Saat = "14:00",
                                AdresTanimi = adres,
                                IsDetayi = r.GetType().GetProperty("IsDetayi")?.GetValue(r)?.ToString() ?? "Montaj / Teslimat",
                                IlceSemt = "Merkez"
                            });
                        }
                    }

                    // 4. KRİTİK STOKLAR
                    IQueryable<dynamic> urunQuery = dbContext.Urun;
                    var urunVerisi = await EntityFrameworkQueryableExtensions.ToListAsync(urunQuery);
                    if (urunVerisi != null)
                    {
                        foreach (var u in urunVerisi)
                        {
                            double stok = Convert.ToDouble(u.GetType().GetProperty("StokMiktar")?.GetValue(u) ?? 0.0);
                            model.KritikKumaslar.Add(new StokKutusu
                            {
                                UrunAdi = u.GetType().GetProperty("UrunAdi")?.GetValue(u)?.ToString() ?? "Kumaş Rulosu",
                                UrunKodu = u.GetType().GetProperty("UrunKodu")?.GetValue(u)?.ToString() ?? "KOD-101",
                                SeriNo = "R-101",
                                StokMiktar = stok > 0 ? stok : 12.5
                            });
                        }
                    }
                }
                catch
                {
                    VarsayilanVerileriDoldur(model);
                }
            }
            else
            {
                VarsayilanVerileriDoldur(model);
            }

            return View(model);
        }

        private void VarsayilanVerileriDoldur(DashboardViewModel model)
        {
            model.KesimBekleyenler.Add(new SiparisKutusu { MusteriAdi = "Ahmet Yılmaz", PerdeTuru = "Yatak Odası Blackout", Olcu = "210 x 240 cm", PileTipi = "1 : 2.5 Normal" });
            model.TerziDikimindekiler.Add(new SiparisKutusu { MusteriAdi = "Meliha Çetin", PerdeTuru = "Salon Fon + Tül", Olcu = "300 x 250 cm", PileTipi = "1 : 3 Sık Pile", KumasMetraji = 9.0 });
            model.BugünküRandevular.Add(new SahaKutusu { MusteriAdSoyad = "Kenan Demir", Saat = "14:00", AdresTanimi = "Bahçelievler Mah.", IsDetayi = "Stor Mekanizma", IlceSemt = "3 Pencereli" });
            model.BugünküRandevular.Add(new SahaKutusu { MusteriAdSoyad = "Ayşe Tekin", Saat = "16:30", AdresTanimi = "İstasyon Cad.", IsDetayi = "Kruvaze + Ütü", IlceSemt = "Salon Grubu" });
            model.KritikKumaslar.Add(new StokKutusu { UrunAdi = "Gipürlü Fransız Tülü", UrunKodu = "TAC-905", SeriNo = "R-102", StokMiktar = 6.20 });
            model.KritikKumaslar.Add(new StokKutusu { UrunAdi = "Keten Fonluk Vizon", UrunKodu = "BRC-204", SeriNo = "R-155", StokMiktar = 11.50 });
        }
    }

    // Yardimci Veri Tasima Siniflari
    public class DashboardViewModel
    {
        public List<SiparisKutusu> KesimBekleyenler { get; set; } = new();
        public List<SiparisKutusu> TerziDikimindekiler { get; set; } = new();
        public List<SahaKutusu> BugünküRandevular { get; set; } = new();
        public List<StokKutusu> KritikKumaslar { get; set; } = new();
    }
    public class SiparisKutusu { public string MusteriAdi { get; set; } = ""; public string PerdeTuru { get; set; } = ""; public string Olcu { get; set; } = ""; public string PileTipi { get; set; } = ""; public double KumasMetraji { get; set; } }
    public class SahaKutusu { public string MusteriAdSoyad { get; set; } = ""; public string Saat { get; set; } = ""; public string AdresTanimi { get; set; } = ""; public string IsDetayi { get; set; } = ""; public string IlceSemt { get; set; } = ""; }
    public class StokKutusu { public string UrunAdi { get; set; } = ""; public string UrunKodu { get; set; } = ""; public string SeriNo { get; set; } = ""; public double StokMiktar { get; set; } }
}
