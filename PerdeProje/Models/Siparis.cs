using System;
namespace PerdeProje.Models
{

    public class Siparis

    {

        public int Id { get; set; }

        public int KullaniciId { get; set; } // Siparişin hangi müşteriye ait olduğunu eşleştirmek için

        public string SiparisKodu { get; set; } = ""; // Örn: #DP-9832

        public string UrunAdi { get; set; } = ""; // Örn: Zebra Perde

        public DateTime SiparisTarihi { get; set; } = DateTime.Now;

        public string Durum { get; set; } = "Atölyede / Dikiliyor"; // Örn: Hazırlanıyor, Teslim Edildi

    }
}