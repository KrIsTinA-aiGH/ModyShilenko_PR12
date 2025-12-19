using Shilenko_wpf1.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shilenko_wpf1.Services
{
    public static class TimeService
    {

        public static string GetTimeBasedGreeting()
        {
            var currentTime = DateTime.Now.TimeOfDay;

            if (currentTime >= new TimeSpan(10, 0, 0) && currentTime <= new TimeSpan(12, 0, 0))
                return "Доброе утро!";
            else if (currentTime >= new TimeSpan(12, 1, 0) && currentTime <= new TimeSpan(17, 0, 0))
                return "Добрый день!";
            else if (currentTime >= new TimeSpan(17, 1, 0) && currentTime <= new TimeSpan(19, 0, 0))
                return "Добрый вечер!";
            else
                return "Добро пожаловать!";
        }

        //проверяет, находится ли текущее время в рабочем интервале
        public static bool IsWithinWorkingHours()
        {
            var currentTime = DateTime.Now.TimeOfDay;
            return currentTime >= new TimeSpan(10, 0, 0) && currentTime <= new TimeSpan(19, 0, 0);
        }

        //получает полное имя пользователя в зависимости от типа
        public static string GetFullUserName(Users user)
        {
            if (user == null) return "Гость";

            try
            {
                using (var db = new AutobaseEntities())
                {
                    //загружаем пользователя с связанными данными
                    var currentUser = db.Users
                        .Include("Employees")
                        .Include("Clients")
                        .FirstOrDefault(u => u.UserID == user.UserID);

                    if (currentUser == null) return user.Email;

                    //для сотрудников
                    if (currentUser.Employees != null)
                    {
                        return GetEmployeeFullName(currentUser.Employees);
                    }
                    //для клиентов
                    else if (currentUser.Clients != null)
                    {
                        return GetClientDisplayName(currentUser.Clients);
                    }
                    //дсли тип не определен
                    else
                    {
                        return user.Email;
                    }
                }
            }
            catch (Exception)
            {
                return user.Email;
            }
        }
        //формирует полное имя сотрудника
        private static string GetEmployeeFullName(Employees employee)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(employee.LastName))
                parts.Add(employee.LastName);
            if (!string.IsNullOrWhiteSpace(employee.FirstName))
                parts.Add(employee.FirstName);

            return parts.Count > 0 ? string.Join(" ", parts) : employee.Email;
        }

        //формирует отображаемое имя клиента
        private static string GetClientDisplayName(Clients client)
        {
            if (!string.IsNullOrWhiteSpace(client.ContactPerson))
                return client.ContactPerson;


            if (!string.IsNullOrWhiteSpace(client.CompanyName))
                return client.CompanyName;

            return client.Email;
        }

        /// Проверяет является ли пользователь сотрудником
        public static bool IsEmployee(Users user)
        {
            if (user == null) return false;

            try
            {
                using (var db = new AutobaseEntities())
                {
                    var currentUser = db.Users
                        .Include("Employees")
                        .FirstOrDefault(u => u.UserID == user.UserID);

                    return currentUser?.Employees != null;
                }
            }
            catch
            {
                return false;
            }
        }

        //проверяет является ли пользователь клиентом
        public static bool IsClient(Users user)
        {
            if (user == null) return false;

            try
            {
                using (var db = new AutobaseEntities())
                {
                    var currentUser = db.Users
                        .Include("Clients")
                        .FirstOrDefault(u => u.UserID == user.UserID);

                    return currentUser?.Clients != null;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}