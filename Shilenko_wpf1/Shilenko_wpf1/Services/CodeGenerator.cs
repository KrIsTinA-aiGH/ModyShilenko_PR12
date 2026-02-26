using System;

namespace Shilenko_wpf1.Services
{
    /// <summary>
    /// Сервис для генерации 4-значных кодов подтверждения.
    /// Используется для двухфакторной аутентификации и восстановления пароля.
    /// </summary>
    public class CodeGenerator
    {
        /// <summary>Генератор случайных чисел</summary>
        private readonly Random random;

        /// <summary>
        /// Конструктор генератора кодов.
        /// Инициализирует генератор случайных чисел.
        /// </summary>
        public CodeGenerator()
        {
            random = new Random();
        }

        /// <summary>
        /// Генерирует 4-значный код подтверждения.
        /// </summary>
        /// <returns>Строка с 4-значным числом от 1000 до 9999</returns>
        public string GenerateCode()
        {
            // Генерируем случайное число в диапазоне от 1000 до 9999
            int code = random.Next(1000, 10000);

            // Преобразуем число в строку и возвращаем
            return code.ToString();
        }
    }
}