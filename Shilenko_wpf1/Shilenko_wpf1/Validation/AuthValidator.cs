using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Shilenko_wpf1.Validators
{
    /// <summary>
    /// Модель валидации для аутентификации пользователя.
    /// Содержит данные для проверки логина, пароля и капчи.
    /// </summary>
    public class AuthValidationModel
    {
        /// <summary>
        /// Email адрес пользователя (логин).
        /// Обязательное поле, формат email, максимум 100 символов.
        /// </summary>
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        [StringLength(100, ErrorMessage = "Email не более 100 символов")]
        public string Email { get; set; }

        /// <summary>
        /// Пароль пользователя.
        /// Обязательное поле, от 6 до 50 символов.
        /// </summary>
        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Минимум 6 символов")]
        [MaxLength(50, ErrorMessage = "Не более 50 символов")]
        public string Password { get; set; }

        /// <summary>Введенное значение капчи пользователем</summary>
        public string Captcha { get; set; }

        /// <summary>Ожидаемое значение капчи для сравнения</summary>
        public string ExpectedCaptcha { get; set; }

        /// <summary>Флаг необходимости проверки капчи</summary>
        public bool CaptchaRequired { get; set; }
    }

    /// <summary>
    /// Валидатор данных аутентификации.
    /// Проверяет корректность логина, пароля и капчи.
    /// </summary>
    public class AuthValidator
    {
        /// <summary>
        /// Выполняет валидацию модели аутентификации.
        /// </summary>
        /// <param name="model">Модель с данными для валидации</param>
        /// <returns>Результат валидации с флагом успеха и списком ошибок</returns>
        public ValidationResultDto Validate(AuthValidationModel model)
        {
            // Создаем список для сбора ошибок валидации
            var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var context = new ValidationContext(model);

            /* 
             * Выполняем стандартную валидацию атрибутов DataAnnotations:
             * [Required], [EmailAddress], [StringLength], [MinLength], [MaxLength]
             */
            bool isValid = Validator.TryValidateObject(model, context, errors, true);

            // Проверка капчи, если она требуется
            if (model.CaptchaRequired)
            {
                /* 
                 * Проверяем капчу:
                 * 1. Поле не должно быть пустым
                 * 2. Введенное значение должно совпадать с ожидаемым
                 */
                if (string.IsNullOrWhiteSpace(model.Captcha))
                    errors.Add(new System.ComponentModel.DataAnnotations.ValidationResult(
                        "Введите капчу", new[] { "Captcha" }));
                else if (model.Captcha != model.ExpectedCaptcha?.Replace(" ", ""))
                    errors.Add(new System.ComponentModel.DataAnnotations.ValidationResult(
                        "Неверная капча", new[] { "Captcha" }));
            }

            // Возвращаем результат валидации
            return new ValidationResultDto
            {
                IsValid = isValid && errors.Count == 0,
                Errors = errors.Select(e => e.ErrorMessage).ToList()
            };
        }
    }
}