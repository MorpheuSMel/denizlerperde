using Microsoft.EntityFrameworkCore;
using PerdeProje.Data;
using PerdeProje.Models;
using PerdeProje.Pages;
using PerdeProje.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=App_Data/perdeproje.db";

var dbPath = connectionString.Replace("Data Source=", "", StringComparison.OrdinalIgnoreCase);
var dbDirectory = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrWhiteSpace(dbDirectory))
{
    Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, dbDirectory));
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    db.Database.EnsureCreated();
    SeedAdmin(db);
    SeedEmployees(db);
    SeedCatalog(db);
    db.SaveChanges();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

app.Run();

static void SeedAdmin(ApplicationDbContext db)
{
    var admin = db.Users.FirstOrDefault(u => u.Email == "admin@admin.com")
        ?? db.Users.FirstOrDefault(u => u.Email == "admin@denizlerperde.com");

    if (admin == null)
    {
        admin = new User { OlusturmaTarihi = DateTime.Now };
        db.Users.Add(admin);
    }

    admin.Ad = "Admin";
    admin.Soyad = "Kullanıcı";
    admin.Email = "admin@admin.com";
    admin.Telefon = "0532 452 11 13";
    admin.Sifre = PasswordHasher.HashPassword("123456");
    admin.Rol = "Admin";
    admin.AktifMi = true;
}

static void SeedEmployees(ApplicationDbContext db)
{
    EnsureEmployee(db, "Fon", "Terzisi", "fon@denizlerperde.com", "FonTerzisi");
    EnsureEmployee(db, "Tül", "Terzisi", "tul@denizlerperde.com", "TulTerzisi");
    EnsureEmployee(db, "Montaj", "Ekibi", "montaj@denizlerperde.com", "Montajci");
    EnsureEmployee(db, "Akıllı Sistem", "Ustası", "akilli@denizlerperde.com", "AkilliSistemci");
    EnsureEmployee(db, "Paketleme", "Personeli", "paket@denizlerperde.com", "Paketlemeci");
    EnsureEmployee(db, "Kargo", "Personeli", "kargo@denizlerperde.com", "Kargocu");
}

static void EnsureEmployee(ApplicationDbContext db, string ad, string soyad, string email, string rol)
{
    var employee = db.Users.FirstOrDefault(u => u.Email == email);

    if (employee == null)
    {
        employee = new User { OlusturmaTarihi = DateTime.Now };
        db.Users.Add(employee);
    }

    employee.Ad = ad;
    employee.Soyad = soyad;
    employee.Email = email;
    employee.Telefon = "0532 452 11 13";
    employee.Sifre = PasswordHasher.HashPassword("Denizler2026!");
    employee.Rol = rol;
    employee.AktifMi = true;
}

static void SeedCatalog(ApplicationDbContext db)
{
    var katalog = SatisModel.CatalogProducts();
    var silinecekAdlar = new HashSet<string>
    {
        "Fon Kartela",
        "Keten Fon Kartela",
        "Dokulu Tul Perde",
        "Su Damlasi Tul Perde",
        "Krem Tul Perde",
        "Dokulu Beyaz Tul Perde",
        "Ekru Tul Perde",
        "Keten Tul Perde",
        "Dikey Tul Perde",
        "Kahverengi Tul Fon Perde",
        "Tul Fon Perde",
        "Gri Tul Fon Perde",
        "Gecisli Stor Perde",
        "Akilli Sistem Blackout",
        "Akilli Sistem Gecisli Tul",
        "Akilli Sistem Mekanizma",
        "Akilli Dikey Jaluzi",
        "Ahsap Jaluzi Perde",
        "Motorlu Akilli Perde",
        "Zebra Perde Beyaz",
        "Zebra Perde Krem",
        "Guneslik Stor Perde",
        "Güneşlik Stor Perde",
        "Blackout Stor Perde",
        "Gipurlu Tul Perde",
        "Gipürlü Örme Tül Perde",
        "Duz Ekru Tul Perde",
        "Keten Fon Perde",
        "Kadife Fon Perde"
    };

    var mevcutUrunler = db.Urunler.ToList();
    var silinecekler = mevcutUrunler.Where(urun => silinecekAdlar.Contains(urun.Ad)).ToList();
    if (silinecekler.Count > 0)
    {
        db.Urunler.RemoveRange(silinecekler);
        mevcutUrunler = mevcutUrunler.Except(silinecekler).ToList();
    }

    foreach (var katalogUrunu in katalog)
    {
        var mevcut = mevcutUrunler.FirstOrDefault(urun => urun.Ad == katalogUrunu.Ad);
        if (mevcut == null)
        {
            db.Urunler.Add(katalogUrunu);
            continue;
        }

        mevcut.Aciklama = katalogUrunu.Aciklama;
        mevcut.Fiyat = katalogUrunu.Fiyat;
        mevcut.ResimUrl = katalogUrunu.ResimUrl;
        mevcut.IkinciResimUrl = katalogUrunu.IkinciResimUrl;
        mevcut.Kategori = katalogUrunu.Kategori;
        mevcut.Stok = katalogUrunu.Stok;
    }
}

