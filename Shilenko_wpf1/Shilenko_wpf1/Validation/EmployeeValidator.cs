using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace Shilenko_wpf1.Validators
{
    /// <summary>
    /// Модель валидации для данных сотрудника.
    /// Содержит все поля формы добавления/редактирования сотрудника.
    /// </summary>
    public class EmployeeValidationModel
    {
        /// <summary>
        /// Фамилия сотрудника.
        /// Обязательное поле, от 2 до 50 символов.
        /// </summary>
        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "От 2 до 50 символов")]
        public string LastName { get; set; }

        /// <summary>
        /// Имя сотрудника.
        /// Обязательное поле, от 2 до 50 символов.
        /// </summary>
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "От 2 до 50 символов")]
        public string FirstName { get; set; }

        /// <summary>
        /// ID должности сотрудника.
        /// Обязательное поле.
        /// </summary>
        [Required(ErrorMessage = "Выберите должность")]
        public int? PositionID { get; set; }

        /// <summary>
        /// Дата приёма сотрудника на работу.
        /// Обязательное поле.
        /// </summary>
        [Required(ErrorMessage = "Укажите дату приема")]
        public DateTime? HireDate { get; set; }

        /// <summary>
        /// Телефон сотрудника.
        /// Должен быть в корректном формате.
        /// </summary>
        [Phone(ErrorMessage = "Неверный формат телефона")]
        public string Phone { get; set; }

        /// <summary>
        /// Email адрес сотрудника.
        /// Обязательное поле, формат email.
        /// </summary>
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string Email { get; set; }

        /// <summary>Путь к файлу изображения сотрудника</summary>
        public string ImagePath { get; set; }
    }

    /// <summary>
    /// Валидатор данных сотрудника.
    /// Проверяет корректность всех полей формы.
    /// </summary>
    public class EmployeeValidator
    {
        /// <summary>
        /// Выполняет валидацию модели сотрудника.
        /// </summary>
        /// <param name="model">Модель с данными для валидации</param>
        /// <returns>Результат валидации с флагом успеха и списком ошибок</returns>
        public ValidationResultDto Validate(EmployeeValidationModel model)
        {
            // Создаем список для сбора ошибок валидации
            var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var context = new ValidationContext(model);

            /* 
             * Выполняем стандартную валидацию атрибутов DataAnnotations:
             * [Required], [StringLength], [Phone], [EmailAddress]
             */
            bool isValid = Validator.TryValidateObject(model, context, errors, true);

            // Дополнительные проверки бизнес-логики

            /* 
             * Проверка даты приёма:
             * Дата не может быть в будущем
             */
            if (model.HireDate.HasValue && model.HireDate > DateTime.Now)
                errors.Add(new System.ComponentModel.DataAnnotations.ValidationResult(
                    "Дата приема не может быть в будущем", new[] { "HireDate" }));

            /* 
             * Проверка размера изображения:
             * Максимальный размер файла - 5MB
             */
            if (!string.IsNullOrEmpty(model.ImagePath) && File.Exists(model.ImagePath))
            {
                var fileInfo = new FileInfo(model.ImagePath);
                if (fileInfo.Length > 5 * 1024 * 1024)
                    errors.Add(new System.ComponentModel.DataAnnotations.ValidationResult(
                        "Размер изображения не должен превышать 5MB", new[] { "ImagePath" }));
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