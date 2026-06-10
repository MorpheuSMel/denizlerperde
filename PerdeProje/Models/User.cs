using System;

namespace PerdeProje.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Ad { get; set; } = ""; // Boş string atayarak uyarıyı kaldırıyoruz
        public string Soyad { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telefon { get; set; } = "";
        public string Sifre { get; set; } = "";
        public string Rol { get; set; } = "Musteri";
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public bool AktifMi { get; set; } = true;
    }
}