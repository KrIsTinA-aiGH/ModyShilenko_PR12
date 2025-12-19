using System;
using System.Security.Cryptography;
using System.Text;

namespace Shilenko_wpf1.Services
{
    public static class Hash
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] sourceBytePassword = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256Hash.ComputeHash(sourceBytePassword);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }
    }
}