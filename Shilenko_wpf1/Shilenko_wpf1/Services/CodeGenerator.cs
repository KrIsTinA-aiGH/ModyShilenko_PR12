using System;

namespace Shilenko_wpf1.Services
{
    ///Сервис для генерации 4-значных кодов подтверждения
    public class CodeGenerator
    {
        ///Генератор случайных чисел
        private readonly Random random;

        ///инициализация генератора случайных чисел
        public CodeGenerator()
        {
            random = new Random();
        }

        ///генерация 4-значного кода подтверждения
        public string GenerateCode()
        {
            int code = random.Next(1000, 10000);
            ///Преобразуем число в строку и возвращаем
            return code.ToString();
        }
    }
}