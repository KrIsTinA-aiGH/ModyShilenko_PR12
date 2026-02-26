using Shilenko_wpf1.Models;
using Shilenko_wpf1.Services;
using System;
using System.Linq;

namespace Shilenko_wpf1.Services
{
    /// <summary>
    /// Сервис для восстановления пароля пользователя.
    /// Обрабатывает генерацию кода, отправку на email и сброс пароля.
    /// </summary>
    public class PasswordRecoveryService
    {
        /// <summary>Генератор кодов подтверждения</summary>
        private readonly CodeGenerator codeGenerator;

        /// <summary>Почтовый сервис для отправки писем</summary>
        private readonly EmailService emailService;

        /// <summary>Текущий сгенерированный код подтверждения</summary>
        private string currentCode;

        /// <summary>Email пользователя, для которого выполняется восстановление пароля</summary>
        private string currentEmail;

        /// <summary>
        /// Конструктор сервиса восстановления пароля.
        /// Инициализирует генератор кодов и почтовый сервис.
        /// </summary>
        public PasswordRecoveryService()
        {
            codeGenerator = new CodeGenerator();
            emailService = new EmailService();
        }

        /// <summary>
        /// Отправляет код восстановления пароля на email пользователя.
        /// </summary>
        /// <param name="email">Email адрес пользователя</param>
        /// <returns>True при успешной отправке, иначе false</returns>
        public bool SendRecoveryCode(string email)
        {
            try
            {
                // Генерируем новый 4-значный код подтверждения
                currentCode = codeGenerator.GenerateCode();

                // Сохраняем email пользователя для последующего использования
                currentEmail = email;

                string subject = "Код восстановления пароля";
                string body = $"Ваш код для восстановления пароля: {currentCode}\n" +
                              $"Код действителен в течение текущей сессии.";

                // Отправляем письмо через почтовый сервис и возвращаем результат
                return emailService.SendEmail(email, subject, body);
            }
            catch (Exception)
            {
                // При любой ошибке возвращаем false
                return false;
            }
        }

        /// <summary>
        /// Проверяет введенный пользователем код подтверждения.
        /// </summary>
        /// <param name="enteredCode">Код, введенный пользователем</param>
        /// <returns>True если код совпадает, иначе false</returns>
        public bool VerifyCode(string enteredCode)
        {
            /* 
             * Проверяем, что текущий код существует и совпадает с введенным.
             * Сравнение регистронезависимое для удобства пользователя.
             */
            return !string.IsNullOrEmpty(currentCode) &&
                   currentCode.Equals(enteredCode, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Сбрасывает пароль пользователя на новый.
        /// </summary>
        /// <param name="newPassword">Новый пароль пользователя</param>
        /// <returns>True при успешном сбросе, иначе false</returns>
        public bool ResetPassword(string newPassword)
        {
            // Проверяем, что email и новый пароль не пустые
            if (string.IsNullOrEmpty(currentEmail) || string.IsNullOrEmpty(newPassword))
                return false;

            try
            {
                using (var db = new AutobaseEntities())
                {
                    // Ищем пользователя в базе данных по email
                    var user = db.Users.FirstOrDefault(u => u.Email == currentEmail);

                    // Если пользователь не найден — возвращаем ошибку
                    if (user == null)
                        return false;

                    /* 
                     * Устанавливаем новый пароль:
                     * - Password хранится в открытом виде (для совместимости)
                     * - PasswordHash хранит хешированную версию для безопасности
                     */
                    user.Password = newPassword;
                    user.PasswordHash = Hash.HashPassword(newPassword);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                // При ошибке базы данных возвращаем false
                return false;
            }
        }
    }
}