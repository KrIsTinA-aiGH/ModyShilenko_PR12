using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Windows;

namespace Shilenko_wpf1.Services
{
    /// <summary>
    /// Сервис для отправки электронных писем через протокол SMTP.
    /// Используется для отправки кодов подтверждения и уведомлений.
    /// </summary>
    public class EmailService
    {
        /// <summary>Сервер SMTP (например, smtp.mail.ru)</summary>
        private readonly string smtpServer;

        /// <summary>Порт SMTP сервера (обычно 587 для Mail.ru)</summary>
        private readonly int smtpPort;

        /// <summary>Логин для аутентификации на SMTP сервере (ваш email)</summary>
        private readonly string smtpUsername;

        /// <summary>Пароль приложения для аутентификации на SMTP сервере</summary>
        private readonly string smtpPassword;

        /// <summary>Флаг включения шифрования SSL/TLS</summary>
        private readonly bool enableSsl;

        /// <summary>
        /// Конструктор сервиса email.
        /// Инициализирует настройки SMTP из файла конфигурации App.config.
        /// </summary>
        public EmailService()
        {
            // Читаем адрес SMTP сервера из конфигурации
            smtpServer = ConfigurationManager.AppSettings["SmtpServer"];

            // Читаем порт SMTP сервера из конфигурации и преобразуем в число
            smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);

            // Читаем логин (email) из конфигурации
            smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"];

            // Читаем пароль приложения из конфигурации
            smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];

            // Читаем флаг включения SSL из конфигурации
            enableSsl = bool.Parse(ConfigurationManager.AppSettings["SmtpEnableSsl"]);
        }

        /// <summary>
        /// Отправляет электронное письмо через SMTP сервер.
        /// </summary>
        /// <param name="toEmail">Адрес получателя письма</param>
        /// <param name="subject">Тема письма</param>
        /// <param name="body">Текст письма</param>
        /// <returns>True при успешной отправке, иначе false</returns>
        public bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                // Создаем клиент SMTP с указанным сервером и портом
                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    // Устанавливаем учетные данные для аутентификации
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                    // Включаем шифрование SSL если требуется
                    client.EnableSsl = enableSsl;

                    /* 
                     * Создаем новое почтовое сообщение со следующими параметрами:
                     * - Отправитель: из конфигурации
                     * - Тема и тело: из параметров метода
                     * - Формат: обычный текст (не HTML)
                     */
                    var mailMessage = new MailMessage
                    {
                        // Устанавливаем отправителя письма (ваш email)
                        From = new MailAddress(smtpUsername),
                        // Устанавливаем тему письма
                        Subject = subject,
                        // Устанавливаем тело письма
                        Body = body,
                        // Указываем, что тело письма в формате обычного текста (не HTML)
                        IsBodyHtml = false
                    };

                    // Добавляем получателя письма
                    mailMessage.To.Add(toEmail);

                    // Отправляем письмо через SMTP сервер
                    client.Send(mailMessage);

                    // Возвращаем успешный результат
                    return true;
                }
            }
            catch (Exception ex)
            {
                // При ошибке показываем сообщение пользователю
                MessageBox.Show($"Ошибка отправки письма: {ex.Message}",
                                "Ошибка SMTP",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                // Возвращаем неудачный результат
                return false;
            }
        }
    }
}