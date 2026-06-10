namespace PerdeProje.Models
{
    public class Favori
    {
        public int Id { get; set; }
        public int KullaniciId { get; set; }
        public int UrunId { get; set; }
        public string UrunAdi { get; set; } = "";

        // İlişki
        public virtual Urun? Urun { get; set; }
    }
}
