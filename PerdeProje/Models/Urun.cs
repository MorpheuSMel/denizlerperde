namespace PerdeProje.Models
{
    public class Urun
    {
        public int Id { get; set; }
        public string Ad { get; set; } = "";
        public string Aciklama { get; set; } = "";
        public decimal Fiyat { get; set; }
        public string ResimUrl { get; set; } = "";
        public string IkinciResimUrl { get; set; } = "";
        public string Kategori { get; set; } = "";
        public int Stok { get; set; }

        // Dashboard altındaki Kritik Rulo Alarmları için CSHTML'in beklediği ek alanlar
        public string UrunAdi => Ad;
        public string UrunKodu => "KMS-" + Id.ToString("D3");
        public string SeriNo => (Id * 123).ToString();
        public int StokMiktar => Stok;
    }
}
