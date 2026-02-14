using Shilenko_wpf1.Models;
using Shilenko_wpf1.Services;
using System;
using System.Linq;

namespace Shilenko_wpf1.Services
{
    public class PasswordRecoveryService
    {
        ///Генератор кодов подтверждения
        private readonly CodeGenerator codeGenerator;
        ///Почтовый сервис для отправки писем
        private readonly EmailService emailService;
        ///Текущий сгенерированный код подтверждения
        private string currentCode;
        ///Email пользователя, для которого выполняется восстановление пароля
        private string currentEmail;

        public PasswordRecoveryService()
        {
            codeGenerator = new CodeGenerator();

            emailService = new EmailService();
        }

        ///отправка кода восстановления пароля на email
        public bool SendRecoveryCode(string email)
        {
            try
            {
                ///Генерируем новый 4-значный код подтверждения
                currentCode = codeGenerator.GenerateCode();
                ///Сохраняем email пользователя для последующего использования
                currentEmail = email;
                string subject = "Код восстановления пароля";
                string body = $"Ваш код для восстановления пароля: {currentCode}\n" +
                              $"Код действителен в течение текущей сессии.";

                ///Отправляем письмо через почтовый сервис и возвращаем результат
                return emailService.SendEmail(email, subject, body);
            }
            catch (Exception)
            {
                return false;
            }
        }

        ///проверка введенного пользователем кода подтверждения
        public bool VerifyCode(string enteredCode)
        {
            ///Проверяем что текущий код существует и совпадает с введенным 
            return !string.IsNullOrEmpty(currentCode) &&
                   currentCode.Equals(enteredCode, StringComparison.OrdinalIgnoreCase);
        }

        ///сброс пароля пользователя на новый
        public bool ResetPassword(string newPassword)
        {
            ///Проверяем что email и новый пароль не пустые
            if (string.IsNullOrEmpty(currentEmail) || string.IsNullOrEmpty(newPassword))
                return false;

            try
            {
                using (var db = new AutobaseEntities())
                {
                    ///Ищем пользователя в базе данных по email
                    var user = db.Users.FirstOrDefault(u => u.Email == currentEmail);
                    ///Если пользователь не найден - возвращаем ошибку
                    if (user == null)
                        return false;

                    ///Устанавливаем новый пароль в открытом виде 
                    user.Password = newPassword;
                    user.PasswordHash = Hash.HashPassword(newPassword);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}