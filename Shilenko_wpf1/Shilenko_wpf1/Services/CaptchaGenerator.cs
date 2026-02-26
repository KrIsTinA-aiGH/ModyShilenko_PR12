using System;

namespace Shilenko_wpf1.Services
{
    /// <summary>
    /// Класс для генерации простой капчи (CAPTCHA).
    /// Используется для защиты от автоматических попыток входа.
    /// </summary>
    public static class SimpleCaptcha
    {
        /// <summary>Генератор случайных чисел для создания капчи</summary>
        private static Random rnd = new Random();

        /// <summary>
        /// Создаёт случайную строку капчи из 6 символов.
        /// Символы включают заглавные буквы латиницы и цифры.
        /// </summary>
        /// <returns>Строка капчи длиной 6 символов</returns>
        public static string Create()
        {
            // Инициализируем пустую строку для результата
            string result = "";

            /* 
             * Генерируем 6 случайных символов:
             * - Используем строку из 36 символов (26 букв + 10 цифр)
             * - rnd.Next(36) выбирает случайный индекс от 0 до 35
             */
            for (int i = 0; i < 6; i++)
            {
                result += "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[rnd.Next(36)];
            }

            return result;
        }
    }
}