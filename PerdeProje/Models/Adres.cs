using System.ComponentModel.DataAnnotations;

namespace PerdeProje.Models
{
    public class Adres
    {
        public int Id { get; set; }

        [Required]
        public string Baslik { get; set; } = string.Empty; // Örn: Ev Adresim

        [Required]
        public string AcikAdres { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }
    }
}