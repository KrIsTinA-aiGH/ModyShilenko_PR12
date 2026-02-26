using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Shilenko_wpf1.Models;
using Shilenko_wpf1.Services;

namespace Shilenko_wpf1.Pages
{
    /// <summary>
    /// Страница клиента — отображает приветствие и информацию о пользователе,
    /// а также предоставляет доступ к функциям управления для администраторов.
    /// </summary>
    public partial class Client : Page
    {
        /// <summary>Текущий авторизованный пользователь</summary>
        private Users _currentUser;

        /// <summary>Роль текущего пользователя в системе</summary>
        private string _userRole;

        /// <summary>
        /// Конструктор страницы клиента.
        /// Инициализирует компоненты и отображает приветствие.
        /// </summary>
        /// <param name="user">Объект пользователя (может быть null для гостя)</param>
        /// <param name="role">Строковое название роли пользователя</param>
        public Client(object user, string role)
        {
            InitializeComponent();

            _currentUser = user as Users;
            _userRole = role;

            // Вызываем метод отображения приветствия
            DisplayUserGreeting();
        }

        /// <summary>
        /// Отображает приветствие и информацию о пользователе.
        /// Учитывает время суток, тип пользователя и его роль.
        /// </summary>
        private void DisplayUserGreeting()
        {
            // Устанавливаем приветствие в зависимости от времени суток
            tbGreeting.Text = TimeService.GetTimeBasedGreeting();

            // Устанавливаем полное имя пользователя
            if (_currentUser != null)
            {
                string fullName = TimeService.GetFullUserName(_currentUser);
                tbUserName.Text = fullName;

                /* 
                 * Определяем тип пользователя:
                 * - Сотрудник, если есть запись в таблице Employees
                 * - Клиент, если есть запись в таблице Clients
                 * - Иначе — обычный пользователь
                 */
                string userType = TimeService.IsEmployee(_currentUser) ? "Сотрудник" :
                                 TimeService.IsClient(_currentUser) ? "Клиент" : "Пользователь";

                tbUserInfo.Text = $"Роль: {_userRole} | {userType}";

                // Если пользователь имеет права администратора — показываем кнопки управления
                if (_userRole == "Администратор" || _userRole == "Менеджер" || _userRole == "Директор")
                {
                    AddAdminButtons();
                }
            }
            else
            {
                // Для гостевого входа
                tbUserName.Text = "Гость";
                tbUserInfo.Text = "Вы вошли в систему как гость";
            }
        }

        /// <summary>
        /// Добавляет кнопки управления для администраторов и менеджеров.
        /// Создаёт панель с кнопками «Сотрудники» и «Заказы».
        /// </summary>
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

            // Кнопка "Заказы" (заглушка для будущей реализации)
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

            // Добавляем кнопки на панель
            buttonPanel.Children.Add(employeesButton);
            buttonPanel.Children.Add(ordersButton);

            // Добавляем панель в ContentPresenter для отображения
            dynamicContent.Content = buttonPanel;
        }

        /// <summary>
        /// Обработчик нажатия кнопки «Управление сотрудниками».
        /// Выполняет навигацию на страницу списка сотрудников.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EmployeesList());
        }

        /// <summary>
        /// Обработчик нажатия кнопки «Управление заказами».
        /// Пока отображает информационное сообщение о будущей реализации.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            // Пока просто сообщение, можно добавить позже
            MessageBox.Show("Функция управления заказами будет реализована позже", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}