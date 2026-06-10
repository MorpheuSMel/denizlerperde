using Microsoft.EntityFrameworkCore;
using PerdeProje.Models;

namespace PerdeProje.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Adres> Adresler { get; set; }
        public DbSet<Kart> Kartlar { get; set; }
        public DbSet<Urun> Urunler { get; set; }
        public DbSet<Favori> Favoriler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
        public DbSet<Siparis> Siparisler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
