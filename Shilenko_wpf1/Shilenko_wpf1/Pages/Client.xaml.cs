using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Shilenko_wpf1.Models;
using Shilenko_wpf1.Services;

namespace Shilenko_wpf1.Pages
{
    public partial class Client : Page
    {
        private Users _currentUser;
        private string _userRole;

        public Client(object user, string role)
        {
            InitializeComponent();

            _currentUser = user as Users;
            _userRole = role;

            DisplayUserGreeting(); //метод приветствия
        }

        private void DisplayUserGreeting()
        {
            //устанавливаем приветствие в зависимости от времени суток
            tbGreeting.Text = TimeService.GetTimeBasedGreeting();

            //устанавливаем полное имя пользователя
            if (_currentUser != null)
            {
                string fullName = TimeService.GetFullUserName(_currentUser);
                tbUserName.Text = fullName;

                //дополнительная информация о пользователе
                string userType = TimeService.IsEmployee(_currentUser) ? "Сотрудник" :
                                 TimeService.IsClient(_currentUser) ? "Клиент" : "Пользователь";

                tbUserInfo.Text = $"Роль: {_userRole} | {userType}";

                if (_userRole == "Администратор" || _userRole == "Менеджер" || _userRole == "Директор")
                {
                    AddAdminButtons();
                }
            }
            else
            {
                tbUserName.Text = "Гость";
                tbUserInfo.Text = "Вы вошли в систему как гость";
            }
        }

        private void AddAdminButtons()
        {
            // Создаем панель с кнопками
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            // Кнопка "Сотрудники"
            Button employeesButton = new Button
            {
                Content = "Управление сотрудниками",
                Width = 200,
                Height = 40,
                Margin = new Thickness(10),
                Background = Brushes.LightBlue,
                FontWeight = FontWeights.Bold
            };
            employeesButton.Click += EmployeesButton_Click;

            // Кнопка "Заказы" (можно добавить позже)
            Button ordersButton = new Button
            {
                Content = "Управление заказами",
                Width = 200,
                Height = 40,
                Margin = new Thickness(10),
                Background = Brushes.LightGreen,
                FontWeight = FontWeights.Bold
            };
            ordersButton.Click += OrdersButton_Click;

            buttonPanel.Children.Add(employeesButton);
            buttonPanel.Children.Add(ordersButton);

            // Добавляем панель в ContentPresenter
            dynamicContent.Content = buttonPanel;
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EmployeesList());
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            // Пока просто сообщение, можно добавить позже
            MessageBox.Show("Функция управления заказами будет реализована позже", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}