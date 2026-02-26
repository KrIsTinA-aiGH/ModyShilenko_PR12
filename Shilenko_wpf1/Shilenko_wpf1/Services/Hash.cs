using System;
using System.Security.Cryptography;
using System.Text;

namespace Shilenko_wpf1.Services
{
    /// <summary>
    /// Класс для хеширования паролей с использованием алгоритма SHA256.
    /// Обеспечивает безопасное хранение паролей в базе данных.
    /// </summary>
    public static class Hash
    {
        /// <summary>
        /// Хеширует пароль с использованием алгоритма SHA256.
        /// </summary>
        /// <param name="password">Исходный пароль в виде строки</param>
        /// <returns>Хеш пароля в виде шестнадцатеричной строки</returns>
        public static string HashPassword(string password)
        {
            // Создаем экземпляр хеш-функции SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                /* 
                 * Преобразуем строку пароля в массив байтов с кодировкой UTF8.
                 * Это необходимо для корректной обработки кириллических символов.
                 */
                byte[] sourceBytePassword = Encoding.UTF8.GetBytes(password);

                // Вычисляем хеш от массива байтов
                byte[] hash = sha256Hash.ComputeHash(sourceBytePassword);

                /* 
                 * Преобразуем массив байтов хеша в шестнадцатеричную строку.
                 * Replace("-", String.Empty) удаляет дефисы из формата BitConverter.
                 */
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }
    }
}