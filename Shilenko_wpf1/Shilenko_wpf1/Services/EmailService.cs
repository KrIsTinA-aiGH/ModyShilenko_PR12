using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Windows;

namespace Shilenko_wpf1.Services
{
    ///Сервис для отправки электронных писем через протокол SMTP
    public class EmailService
    {
        ///Сервер SMTP (например, smtp.mail.ru)
        private readonly string smtpServer;
        ///Порт SMTP сервера (обычно 587 для Mail.ru)
        private readonly int smtpPort;
        ///Логин для аутентификации на SMTP сервере (ваш email)
        private readonly string smtpUsername;
        ///Пароль приложения для аутентификации на SMTP сервере
        private readonly string smtpPassword;
        ///Флаг включения шифрования SSL/TLS
        private readonly bool enableSsl;

        ///Конструктор: инициализация настроек SMTP из файла конфигурации App.config
        public EmailService()
        {
            ///Читаем адрес SMTP сервера из конфигурации
            smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
            ///Читаем порт SMTP сервера из конфигурации и преобразуем в число
            smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            ///Читаем логин (email) из конфигурации
            smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"];
            ///Читаем пароль приложения из конфигурации
            smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
            ///Читаем флаг включения SSL из конфигурации
            enableSsl = bool.Parse(ConfigurationManager.AppSettings["SmtpEnableSsl"]);
        }

        ///Метод: отправка электронного письма
        ///toEmail - адрес получателя письма
        ///subject - тема письма
        ///body - текст письма
        ///Возвращает true при успешной отправке, иначе false
        public bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                ///Создаем клиент SMTP с указанным сервером и портом
                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    ///Устанавливаем учетные данные для аутентификации
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    ///Включаем шифрование SSL если требуется
                    client.EnableSsl = enableSsl;

                    ///Создаем новое почтовое сообщение
                    var mailMessage = new MailMessage
                    {
                        ///Устанавливаем отправителя письма (ваш email)
                        From = new MailAddress(smtpUsername),
                        ///Устанавливаем тему письма
                        Subject = subject,
                        ///Устанавливаем тело письма
                        Body = body,
                        ///Указываем что тело письма в формате обычного текста (не HTML)
                        IsBodyHtml = false
                    };
                    ///Добавляем получателя письма
                    mailMessage.To.Add(toEmail);

                    ///Отправляем письмо через SMTP сервер
                    client.Send(mailMessage);
                    ///Возвращаем успешный результат
                    return true;
                }
            }
            catch (Exception ex)
            {
                ///При ошибке показываем сообщение пользователю
                MessageBox.Show($"Ошибка отправки письма: {ex.Message}",
                               "Ошибка SMTP",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
                ///Возвращаем неудачный результат
                return false;
            }
        }
    }
}