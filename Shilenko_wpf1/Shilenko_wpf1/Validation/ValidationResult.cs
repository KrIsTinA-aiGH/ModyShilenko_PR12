using System.Collections.Generic;

namespace Shilenko_wpf1.Validators
{
    // Переименовал класс, чтобы не конфликтовал
    public class ValidationResultDto
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public string ErrorMessage => string.Join("\n", Errors);

        public ValidationResultDto()
        {
            IsValid = true;
        }
    }
}