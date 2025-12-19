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
    public partial class EmployeesList : Page
    {
        private AutobaseEntities _db;
        private List<Employees> _allEmployees;

        public EmployeesList()
        {
            InitializeComponent();
            LoadData();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }


        private void LoadData()
        {
            try
            {
                if (_db != null)
                {
                    _db.Dispose();
                }

                _db = new AutobaseEntities();
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

        private void UpdateStatus()
        {
            int count = lvEmployees.Items.Count;
            tbStatus.Text = $"Найдено сотрудников: {count}";
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cmbPositionFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allEmployees == null) return;

            var filtered = _allEmployees.AsEnumerable();

            // Применяем поиск по ФИО
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

            lvEmployees.ItemsSource = filtered.ToList();
            UpdateStatus();
        }

        private void btnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу добавления сотрудника (режим создания)
            NavigationService.Navigate(new EmployeeEdit(null));
        }

        private void lvEmployees_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Переход на страницу редактирования сотрудника
            var selectedEmployee = lvEmployees.SelectedItem as Employees;
            if (selectedEmployee != null)
            {
                NavigationService.Navigate(new EmployeeEdit(selectedEmployee));
            }
        }
    }
}