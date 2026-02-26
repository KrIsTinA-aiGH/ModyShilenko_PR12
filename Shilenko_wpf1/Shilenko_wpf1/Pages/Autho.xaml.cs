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
        ///сервис восстановления пароля для обработки сброса пароля
        private PasswordRecoveryService passwordRecoveryService;
        ///генератор кодов для создания кодов 2FA и восстановления пароля
        private CodeGenerator codeGenerator;
        ///текущий код двухфакторной аутентификации
        private string current2FACode;
        ///текущий пользователь для которого выполняется 2FA
        private Users currentUserFor2FA;

        int attempts = 0;
        private bool isBlocked = false;
        private bool captchaRequired = false;
        private System.Windows.Threading.DispatcherTimer blockTimer;

        //Конструктор страницы авторизации
        public Autho()
        {
            InitializeComponent();
            passwordRecoveryService = new PasswordRecoveryService();
            codeGenerator = new CodeGenerator();
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
                        if (!TimeService.IsWithinWorkingHours() && TimeService.IsEmployee(user))
                        {
                            MessageBox.Show("Доступ разрешен только в рабочее время (10:00-19:00)!");
                            return;
                        }

                        currentUserFor2FA = user;

                        if (!cbDisable2FA.IsChecked.Value)  //Если флажок НЕ установлен 
                        {
                            PerformTwoFactorAuthentication(user);  //Запускаем 2FA
                        }
                        else
                        {
                            LoginSuccess(user);  //Пропускаем 2FA и сразу авторизуем
                        }

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

        ///выполнение двухфакторной аутентификации
        private void PerformTwoFactorAuthentication(Users user)
        {
            ///Получаем email пользователя для отправки кода
            string email = user.Email;
            ///Генерируем новый 4-значный код 2FA
            current2FACode = codeGenerator.GenerateCode();

            var emailService = new EmailService();
            ///Отправляем код 2FA на email пользователя
            bool codeSent = emailService.SendEmail(
                email,
                "Код двухфакторной аутентификации",
                $"Ваш код для входа: {current2FACode}\n" +
                $"Код действителен в течение текущей сессии."
            );

            ///Проверяем результат отправки письма
            if (!codeSent)
            {
                MessageBox.Show("Не удалось отправить код аутентификации. Попробуйте ещё раз.",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
                return;
            }

            ///Создаем и показываем диалог ввода кода 2FA
            var twoFactorDialog = new TwoFactorDialog();
            ///Проверяем что пользователь ввел код
            if (twoFactorDialog.ShowDialog() == true)
            {
                ///Получаем введенный код
                string enteredCode = twoFactorDialog.EnteredCode;
                ///Проверяем правильность кода (без учета регистра)
                if (current2FACode.Equals(enteredCode, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Код подтверждён!",
                                   "Успех",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                    ///Выполняем успешный вход пользователя
                    LoginSuccess(currentUserFor2FA);
                }
                else
                {
                    MessageBox.Show("Неверный код аутентификации",
                                   "Ошибка",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Вход отменён",
                               "Информация",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
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

            //проверяем по email
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

        ///обработка нажатия кнопки "Забыли пароль?"
        private void btnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            ///Создаем и показываем диалог ввода email
            var recoveryDialog = new PasswordRecoveryDialog();
            ///Проверяем что пользователь подтвердил ввод email
            if (recoveryDialog.ShowDialog() == true)
            {
                ///Получаем введенный email
                string email = recoveryDialog.EnteredEmail;

                using (var db = new AutobaseEntities())
                {
                    var user = db.Users.FirstOrDefault(u => u.Email == email);
                    if (user == null)
                    {
                        MessageBox.Show("Пользователь с таким email не найден",
                                       "Ошибка",
                                       MessageBoxButton.OK,
                                       MessageBoxImage.Warning);
                        return;
                    }
                }

                ///Отправляем код восстановления на указанный email
                bool codeSent = passwordRecoveryService.SendRecoveryCode(email);
                if (!codeSent)
                {
                    MessageBox.Show("Не удалось отправить код на указанный email",
                                   "Ошибка",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Error);
                    return;
                }

                ///Создаем и показываем диалог ввода кода подтверждения
                var codeDialog = new CodeVerificationDialog();
                ///Проверяем что пользователь ввел код
                if (codeDialog.ShowDialog() == true)
                {
                    ///Получаем введенный код
                    string enteredCode = codeDialog.EnteredCode;
                    ///Проверяем правильность кода
                    if (passwordRecoveryService.VerifyCode(enteredCode))
                    {
                        ///Создаем и показываем диалог ввода нового пароля
                        var resetDialog = new ResetPasswordDialog();
                        ///Проверяем что пользователь подтвердил новый пароль
                        if (resetDialog.ShowDialog() == true)
                        {
                            ///Получаем новый пароль
                            string newPassword = resetDialog.NewPassword;
                            ///Сбрасываем пароль в базе данных
                            bool success = passwordRecoveryService.ResetPassword(newPassword);

                            if (success)
                            {
                                MessageBox.Show("Пароль успешно изменён! Теперь вы можете войти с новым паролем",
                                               "Успех",
                                               MessageBoxButton.OK,
                                               MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Не удалось изменить пароль",
                                               "Ошибка",
                                               MessageBoxButton.OK,
                                               MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Неверный код подтверждения",
                                       "Ошибка",
                                       MessageBoxButton.OK,
                                       MessageBoxImage.Warning);
                    }
                }
            }
        }


    }
}