using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace Shilenko_wpf1.Validators
{
    public class EmployeeValidationModel
    {
        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "От 2 до 50 символов")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "От 2 до 50 символов")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Выберите должность")]
        public int? PositionID { get; set; }

        [Required(ErrorMessage = "Укажите дату приема")]
        public DateTime? HireDate { get; set; }

        [Phone(ErrorMessage = "Неверный формат телефона")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string Email { get; set; }

        public string ImagePath { get; set; }
    }

    public class EmployeeValidator
    {
        public ValidationResultDto Validate(EmployeeValidationModel model)
        {
            var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var context = new ValidationContext(model);

            bool isValid = Validator.TryValidateObject(model, context, errors, true);

            // Дополнительные проверки
            if (model.HireDate.HasValue && model.HireDate > DateTime.Now)
                errors.Add(new System.ComponentModel.DataAnnotations.ValidationResult(
                    "Дата приема не может быть в будущем", new[] { "HireDate" }));

            if (!string.IsNullOrEmpty(model.ImagePath) && File.Exists(model.ImagePath))
            {
                var fileInfo = new FileInfo(model.ImagePath);
                if (fileInfo.Length > 5 * 1024 * 1024)
                    errors.Add(new System.ComponentModel.DataAnnotations.ValidationResult(
                        "Размер изображения не должен превышать 5MB", new[] { "ImagePath" }));
            }

            return new ValidationResultDto
            {
                IsValid = isValid && errors.Count == 0,
                Errors = errors.Select(e => e.ErrorMessage).ToList()
            };
        }
    }
}