using Shilenko_wpf1.Models;
using Shilenko_wpf1.Services;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Threading.Tasks;
using System;
using System.Data.Entity;
using Shilenko_wpf1.Validators;

namespace Shilenko_wpf1.Pages
{
    public partial class Autho : Page
    {
        int attempts = 0;
        private bool isBlocked = false;
        private bool captchaRequired = false;
        private System.Windows.Threading.DispatcherTimer blockTimer;

        public Autho()
        {
            InitializeComponent();
            InitializeTimer();
            ResetForm();
        }

        private void InitializeTimer()
        {
            blockTimer = new System.Windows.Threading.DispatcherTimer();
            blockTimer.Interval = TimeSpan.FromSeconds(1);
            blockTimer.Tick += BlockTimer_Tick;
        }

        private void btnEnterGuest_Click(object sender, RoutedEventArgs e)
        {
            if (!isBlocked)
                NavigationService.Navigate(new Client(null, "Гость"));
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            if (isBlocked) return;

            var model = new AuthValidationModel
            {
                Email = tbLogin.Text?.Trim(),
                Password = tbPassword.Password?.Trim(),
                Captcha = tbCaptcha.Text?.Trim(),
                ExpectedCaptcha = tblCaptcha.Text?.Replace(" ", ""),
                CaptchaRequired = captchaRequired
            };

            var validator = new AuthValidator();
            var result = validator.Validate(model);

            if (!result.IsValid)
            {
                MessageBox.Show($"Ошибки:\n{result.ErrorMessage}",
                               "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isBlocked) return;

            //проверка капчи, если она требуется
            if (captchaRequired)
            {
                if (string.IsNullOrWhiteSpace(tbCaptcha.Text) || tbCaptcha.Text != tblCaptcha.Text.Replace(" ", ""))
                {
                    MessageBox.Show("Неверная капча!");
                    ShowCaptcha();
                    return;
                }
            }

            attempts++;
            var login = tbLogin.Text.Trim();
            var password = tbPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль!");
                attempts--;
                return;
            }



            try
            {
                using (var db = new AutobaseEntities())
                {
                    var user = db.Users.FirstOrDefault(x => x.Email == login && x.Password == password);

                    if (user != null)
                    {
                        //проверка рабочего времени только для сотрудников (не для клиентов)
                        if (!TimeService.IsWithinWorkingHours() && TimeService.IsEmployee(user))
                        {
                            MessageBox.Show("Доступ разрешен только в рабочее время (10:00-19:00)!");
                            return;
                        }

                        LoginSuccess(user);
                        attempts = 0;
                        captchaRequired = false;
                        HideCaptcha();
                    }
                    else
                    {
                        ShowErrorAndCaptcha();

                        if (attempts >= 4)
                        {
                            BlockForm(10);
                        }
                    }
                }
            }
            catch
            {
                if (!isBlocked)
                    NavigationService.Navigate(new Client(null, "Гость"));
            }
        }


        private void LoginSuccess(Users user)
        {
            var role = GetRole(user);
            MessageBox.Show($"Вы вошли как: {role}");
            NavigationService.Navigate(new Client(user, role));
        }

        private void ShowErrorAndCaptcha()
        {
            MessageBox.Show("Неверный логин или пароль!");

            captchaRequired = true;
            ShowCaptcha();
            tbPassword.Clear();
        }

        private void ShowCaptcha()
        {
            tbCaptcha.Visibility = Visibility.Visible;
            tblCaptcha.Visibility = Visibility.Visible;
            tblCaptcha.Text = SimpleCaptcha.Create();
            tblCaptcha.TextDecorations = TextDecorations.Strikethrough;
            tbCaptcha.Clear();
        }

        private void HideCaptcha()
        {
            tbCaptcha.Visibility = Visibility.Hidden;
            tblCaptcha.Visibility = Visibility.Hidden;
            captchaRequired = false;
        }

        private string GetRole(Users user)
        {
            try
            {
                using (var db = new AutobaseEntities())
                {
                    // Загружаем пользователя с связанными данными
                    var currentUser = db.Users
                        .Include("Employees.EmployeePositions")
                        .FirstOrDefault(u => u.UserID == user.UserID);

                    // Если пользователь - сотрудник, проверяем его должность
                    if (currentUser?.Employees != null)
                    {
                        var positionName = currentUser.Employees.EmployeePositions?.PositionName;

                        if (!string.IsNullOrEmpty(positionName))
                        {
                            // Определяем роль по должности
                            if (positionName.Contains("Директор")) return "Директор";
                            if (positionName.Contains("Менеджер")) return "Менеджер";
                            if (positionName.Contains("Водитель")) return "Водитель";
                            if (positionName.Contains("Диспетчер")) return "Диспетчер";
                            if (positionName.Contains("Механик")) return "Механик";
                        }

                        return "Сотрудник";
                    }
                    // Если пользователь - клиент
                    else if (currentUser?.Clients != null)
                    {
                        return "Клиент";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка определения роли: {ex.Message}");
            }

            // Fallback: проверяем по email
            var email = user.Email.ToLower();
            if (email.Contains("admin")) return "Администратор";
            if (email.Contains("director")) return "Директор";
            if (email.Contains("manager")) return "Менеджер";
            if (email.Contains("driver")) return "Водитель";
            if (email.Contains("dispatcher")) return "Диспетчер";
            if (email.Contains("mechanic")) return "Механик";
            if (email.Contains("client")) return "Клиент";

            return "Пользователь";
        }

        private void ResetForm()
        {
            if (!isBlocked)
            {
                tbLogin.Clear();
                tbPassword.Clear();
                tbCaptcha.Clear();
                HideCaptcha();
            }
        }

        private void BlockForm(int seconds)
        {
            isBlocked = true;
            remainingBlockTime = seconds;

            tbLogin.IsEnabled = false;
            tbPassword.IsEnabled = false;
            tbCaptcha.IsEnabled = false;
            btnEnter.IsEnabled = false;
            btnEnterGuest.IsEnabled = false;

            tbTimer.Visibility = Visibility.Visible;
            UpdateTimerText();

            blockTimer.Start();
        }

        private int remainingBlockTime = 0;

        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            remainingBlockTime--;
            UpdateTimerText();

            if (remainingBlockTime <= 0)
            {
                UnblockForm();
            }
        }

        private void UpdateTimerText()
        {
            tbTimer.Text = $"До разблокировки осталось: {remainingBlockTime} сек.";
        }

        private void UnblockForm()
        {
            blockTimer.Stop();
            isBlocked = false;
            attempts = 0;
            captchaRequired = false;

            tbLogin.IsEnabled = true;
            tbPassword.IsEnabled = true;
            tbCaptcha.IsEnabled = true;
            btnEnter.IsEnabled = true;
            btnEnterGuest.IsEnabled = true;

            tbTimer.Visibility = Visibility.Collapsed;

            ResetForm();
        }
    }
}