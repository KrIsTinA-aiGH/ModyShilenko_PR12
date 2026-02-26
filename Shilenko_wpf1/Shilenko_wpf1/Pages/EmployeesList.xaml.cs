using Shilenko_wpf1.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Shilenko_wpf1.Pages
{
    /// <summary>
    /// Страница списка сотрудников.
    /// Отображает таблицу сотрудников с возможностью поиска, фильтрации
    /// и перехода к добавлению/редактированию записей.
    /// </summary>
    public partial class EmployeesList : Page
    {
        /// <summary>Контекст базы данных для работы с сущностями</summary>
        private AutobaseEntities _db;

        /// <summary>Кэш всех сотрудников для фильтрации на клиенте</summary>
        private List<Employees> _allEmployees;

        /// <summary>
        /// Конструктор страницы списка сотрудников.
        /// Инициализирует компоненты и загружает данные.
        /// </summary>
        public EmployeesList()
        {
            InitializeComponent();
            LoadData();
        }

        /// <summary>
        /// Обработчик события загрузки страницы.
        /// Обновляет данные при каждом отображении страницы.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// Загружает данные сотрудников из базы данных.
        /// Заполняет ListView и ComboBox фильтра должностей.
        /// </summary>
        private void LoadData()
        {
            try
            {
                // Освобождаем ресурсы предыдущего контекста, если есть
                if (_db != null)
                {
                    _db.Dispose();
                }

                _db = new AutobaseEntities();

                // Загружаем сотрудников с подгрузкой связанных должностей
                _allEmployees = _db.Employees.Include("EmployeePositions").ToList();

                lvEmployees.ItemsSource = _allEmployees;

                // Заполняем фильтр должностей
                var positions = _db.EmployeePositions.ToList();
                cmbPositionFilter.Items.Clear();
                cmbPositionFilter.Items.Add("Все должности");
                foreach (var pos in positions)
                {
                    cmbPositionFilter.Items.Add(pos.PositionName);
                }
                cmbPositionFilter.SelectedIndex = 0;

                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновляет текст статусной строки с количеством найденных сотрудников.
        /// </summary>
        private void UpdateStatus()
        {
            int count = lvEmployees.Items.Count;
            tbStatus.Text = $"Найдено сотрудников: {count}";
        }

        /// <summary>
        /// Обработчик изменения текста в поле поиска.
        /// Применяет фильтры к списку сотрудников.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        /// <summary>
        /// Обработчик изменения выбранного элемента в фильтре должностей.
        /// Применяет фильтры к списку сотрудников.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void cmbPositionFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        /// <summary>
        /// Применяет фильтры поиска по ФИО и должности к списку сотрудников.
        /// </summary>
        private void ApplyFilters()
        {
            if (_allEmployees == null) return;

            var filtered = _allEmployees.AsEnumerable();

            // Применяем поиск по ФИО (регистронезависимый)
            string searchText = txtSearch.Text?.ToLower();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(emp =>
                    (emp.LastName?.ToLower().Contains(searchText) ?? false) ||
                    (emp.FirstName?.ToLower().Contains(searchText) ?? false) ||
                    ((emp.LastName + " " + emp.FirstName).ToLower().Contains(searchText)));
            }

            // Применяем фильтр по должности
            if (cmbPositionFilter.SelectedIndex > 0)
            {
                string selectedPosition = cmbPositionFilter.SelectedItem.ToString();
                filtered = filtered.Where(emp =>
                    emp.EmployeePositions?.PositionName == selectedPosition);
            }

            // Обновляем источник данных ListView
            lvEmployees.ItemsSource = filtered.ToList();
            UpdateStatus();
        }

        /// <summary>
        /// Обработчик нажатия кнопки «Добавить сотрудника».
        /// Выполняет навигацию на страницу создания нового сотрудника.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void btnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу добавления сотрудника (режим создания)
            NavigationService.Navigate(new EmployeeEdit(null));
        }

        /// <summary>
        /// Обработчик двойного клика по элементу списка сотрудников.
        /// Выполняет навигацию на страницу редактирования выбранного сотрудника.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void lvEmployees_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Получаем выбранного сотрудника
            var selectedEmployee = lvEmployees.SelectedItem as Employees;
            if (selectedEmployee != null)
            {
                // Переход на страницу редактирования с передачей объекта
                NavigationService.Navigate(new EmployeeEdit(selectedEmployee));
            }
        }
    }
}