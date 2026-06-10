using System;
using System.Security.Cryptography;
using System.Text;

namespace PerdeProje.Services
{
    public static class PasswordHasher
    {
        // Mevcut şifreleme metodunuz
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Giriş yaparken hata veren, eksik olan şifre doğrulama metodumuz (CS0117 Çözümü)
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string hashedInput = HashPassword(password);
            return string.Equals(hashedInput, hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}