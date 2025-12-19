using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Shilenko_wpf1.Validators
{
    public class AuthValidationModel
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        [StringLength(100, ErrorMessage = "Email не более 100 символов")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Минимум 6 символов")]
        [MaxLength(50, ErrorMessage = "Не более 50 символов")]
        public string Password { get; set; }

        public string Captcha { get; set; }
        public string ExpectedCaptcha { get; set; }
        public bool CaptchaRequired { get; set; }
    }

    public class AuthValidator
    {
        public ValidationResultDto Validate(AuthValidationModel model)
        {
            var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var context = new ValidationContext(model);

            bool isValid = Validator.TryValidateObject(model, context, errors, true);

            // Проверка капчи
            if (model.CaptchaRequired)
            {
                if (string.IsNullOrWhiteSpace(model.Captcha))
                    errors.Add(new System.ComponentModel.DataAnnotations.ValidationResult(
                        "Введите капчу", new[] { "Captcha" }));
                else if (model.Captcha != model.ExpectedCaptcha?.Replace(" ", ""))
                    errors.Add(new System.ComponentModel.DataAnnotations.ValidationResult(
                        "Неверная капча", new[] { "Captcha" }));
            }

            return new ValidationResultDto
            {
                IsValid = isValid && errors.Count == 0,
                Errors = errors.Select(e => e.ErrorMessage).ToList()
            };
        }
    }
}