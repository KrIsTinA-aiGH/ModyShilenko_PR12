using Shilenko_wpf1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace Shilenko_wpf1.Services
{
    /// <summary>
    /// Сервис для работы со временем и определения типа пользователя.
    /// Содержит методы для приветствия по времени суток, проверки рабочего времени
    /// и определения роли пользователя (сотрудник/клиент).
    /// </summary>
    public static class TimeService
    {
        /// <summary>
        /// Возвращает приветствие в зависимости от текущего времени суток.
        /// </summary>
        /// <returns>Строка с приветствием</returns>
        public static string GetTimeBasedGreeting()
        {
            // Получаем текущее время суток
            var currentTime = DateTime.Now.TimeOfDay;

            /* 
             * Определяем время суток и возвращаем соответствующее приветствие:
             * - Утро: 10:00 - 12:00
             * - День: 12:01 - 17:00
             * - Вечер: 17:01 - 19:00
             * - Ночь/вне рабочего времени: "Добро пожаловать!"
             */
            if (currentTime >= new TimeSpan(10, 0, 0) && currentTime <= new TimeSpan(12, 0, 0))
                return "Доброе утро!";
            else if (currentTime >= new TimeSpan(12, 1, 0) && currentTime <= new TimeSpan(17, 0, 0))
                return "Добрый день!";
            else if (currentTime >= new TimeSpan(17, 1, 0) && currentTime <= new TimeSpan(19, 0, 0))
                return "Добрый вечер!";
            else
                return "Добро пожаловать!";
        }

        /// <summary>
        /// Проверяет, находится ли текущее время в рабочем интервале.
        /// Рабочее время: с 10:00 до 19:00.
        /// </summary>
        /// <returns>True если текущее время в рабочем интервале, иначе false</returns>
        public static bool IsWithinWorkingHours()
        {
            // Получаем текущее время суток
            var currentTime = DateTime.Now.TimeOfDay;

            // Проверяем是否在 рабочем интервале (10:00 - 19:00)
            return currentTime >= new TimeSpan(10, 0, 0) && currentTime <= new TimeSpan(19, 0, 0);
        }

        /// <summary>
        /// Получает полное имя пользователя в зависимости от его типа.
        /// Для сотрудников возвращает ФИО, для клиентов — контактное лицо или название компании.
        /// </summary>
        /// <param name="user">Объект пользователя</param>
        /// <returns>Полное имя или email пользователя</returns>
        public static string GetFullUserName(Users user)
        {
            // Если пользователь null — возвращаем "Гость"
            if (user == null) return "Гость";

            try
            {
                using (var db = new AutobaseEntities())
                {
                    // Загружаем пользователя с связанными данными (Employees и Clients)
                    var currentUser = db.Users
                        .Include("Employees")
                        .Include("Clients")
                        .FirstOrDefault(u => u.UserID == user.UserID);

                    if (currentUser == null) return user.Email;

                    // Для сотрудников — формируем ФИО
                    if (currentUser.Employees != null)
                    {
                        return GetEmployeeFullName(currentUser.Employees);
                    }
                    // Для клиентов — возвращаем контактное лицо или название компании
                    else if (currentUser.Clients != null)
                    {
                        return GetClientDisplayName(currentUser.Clients);
                    }
                    // Если тип не определен — возвращаем email
                    else
                    {
                        return user.Email;
                    }
                }
            }
            catch (Exception)
            {
                // При ошибке возвращаем email пользователя
                return user.Email;
            }
        }

        /// <summary>
        /// Формирует полное имя сотрудника из фамилии и имени.
        /// </summary>
        /// <param name="employee">Объект сотрудника</param>
        /// <returns>Полное имя сотрудника или email</returns>
        private static string GetEmployeeFullName(Employees employee)
        {
            // Создаем список для частей имени
            var parts = new List<string>();

            // Добавляем фамилию если не пустая
            if (!string.IsNullOrWhiteSpace(employee.LastName))
                parts.Add(employee.LastName);

            // Добавляем имя если не пустое
            if (!string.IsNullOrWhiteSpace(employee.FirstName))
                parts.Add(employee.FirstName);

            // Возвращаем объединённое имя или email если части пустые
            return parts.Count > 0 ? string.Join("  ", parts) : employee.Email;
        }

        /// <summary>
        /// Формирует отображаемое имя клиента.
        /// Приоритет: контактное лицо → название компании → email.
        /// </summary>
        /// <param name="client">Объект клиента</param>
        /// <returns>Отображаемое имя клиента</returns>
        private static string GetClientDisplayName(Clients client)
        {
            // Возвращаем контактное лицо если указано
            if (!string.IsNullOrWhiteSpace(client.ContactPerson))
                return client.ContactPerson;

            // Возвращаем название компании если указано
            if (!string.IsNullOrWhiteSpace(client.CompanyName))
                return client.CompanyName;

            // Возвращаем email как резервный вариант
            return client.Email;
        }

        /// <summary>
        /// Проверяет, является ли пользователь сотрудником.
        /// </summary>
        /// <param name="user">Объект пользователя для проверки</param>
        /// <returns>True если пользователь является сотрудником, иначе false</returns>
        public static bool IsEmployee(Users user)
        {
            // Null-проверка: если пользователь null — возвращаем false
            if (user == null) return false;

            try
            {
                using (var db = new AutobaseEntities())
                {
                    // Загружаем пользователя с данными о сотруднике
                    var currentUser = db.Users
                        .Include("Employees")
                        .FirstOrDefault(u => u.UserID == user.UserID);

                    // Возвращаем true если есть связанная запись в Employees
                    return currentUser?.Employees != null;
                }
            }
            catch
            {
                // При ошибке возвращаем false
                return false;
            }
        }

        /// <summary>
        /// Проверяет, является ли пользователь клиентом.
        /// </summary>
        /// <param name="user">Объект пользователя для проверки</param>
        /// <returns>True если пользователь является клиентом, иначе false</returns>
        public static bool IsClient(Users user)
        {
            // Null-проверка: если пользователь null — возвращаем false
            if (user == null) return false;

            try
            {
                using (var db = new AutobaseEntities())
                {
                    // Загружаем пользователя с данными о клиенте
                    var currentUser = db.Users
                        .Include("Clients")
                        .FirstOrDefault(u => u.UserID == user.UserID);

                    // Возвращаем true если есть связанная запись в Clients
                    return currentUser?.Clients != null;
                }
            }
            catch
            {
                // При ошибке возвращаем false
                return false;
            }
        }
    }
}