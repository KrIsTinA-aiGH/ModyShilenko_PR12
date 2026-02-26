using System.Collections.Generic;

namespace Shilenko_wpf1.Validators
{
    /// <summary>
    /// Класс-объект для передачи результатов валидации.
    /// Содержит флаг успешности и список ошибок.
    /// Переименован в ValidationResultDto для избежания конфликтов с System.ComponentModel.DataAnnotations.ValidationResult.
    /// </summary>
    public class ValidationResultDto
    {
        /// <summary>Флаг успешности валидации (true если ошибок нет)</summary>
        public bool IsValid { get; set; }

        /// <summary>Список сообщений об ошибках валидации</summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Объединённое сообщение об ошибках.
        /// Все ошибки разделены символом новой строки.
        /// </summary>
        public string ErrorMessage => string.Join("\n", Errors);

        /// <summary>
        /// Конструктор по умолчанию.
        /// Инициализирует результат как успешный (IsValid = true).
        /// </summary>
        public ValidationResultDto()
        {
            IsValid = true;
        }
    }
}