using System;

namespace PerdeProje.Models
{
    public class Randevu
    {
        public int Id { get; set; }
        public int? KullaniciId { get; set; }
        public string MusteriAdi { get; set; } = "";
        public string PerdeTuru { get; set; } = "";
        public string Olcu { get; set; } = ""; // Arayüzde SAAT/ZAMAN bilgisi için kullanılıyor
        public string PileTipi { get; set; } = ""; // Arayüzde ADRES / NOT bilgisi için kullanılıyor
        public string DurumEtiketi { get; set; } = "Yeni Talep"; // Örn: Yeni Talep, Ölçü Alındı
    }
}
