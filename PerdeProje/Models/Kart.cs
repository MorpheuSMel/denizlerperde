using System.ComponentModel.DataAnnotations;

namespace PerdeProje.Models
{
    public class Kart
    {
        public int Id { get; set; }

        [Required]
        public string KartBasligi { get; set; } = string.Empty; // Örn: Bonus Kartım

        [Required]
        public string KartUzerindekiIsim { get; set; } = string.Empty; // Örn: AHMET YILMAZ

        [Required]
        public string KartNumarasi { get; set; } = string.Empty;

        [Required]
        public string SonKullanmaTarihi { get; set; } = string.Empty; // Örn: 12/28

        [Required]
        public string Cvv { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }
    }
}