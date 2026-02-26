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
    /// <summary>
    /// Страница авторизации пользователя в системе «Автобаза».
    /// Обрабатывает вход по логину/паролю, двухфакторную аутентификацию,
    /// восстановление пароля и защиту от подбора учётных данных.
    /// </summary>
    public partial class Autho : Page
    {
        /// <summary>Сервис восстановления пароля для обработки сброса пароля</summary>
        private PasswordRecoveryService passwordRecoveryService;

        /// <summary>Генератор кодов для создания кодов 2FA и восстановления пароля</summary>
        private CodeGenerator codeGenerator;

        /// <summary>Текущий код двухфакторной аутентификации</summary>
        private string current2FACode;

        /// <summary>Текущий пользователь, для которого выполняется 2FA</summary>
        private Users currentUserFor2FA;

        /// <summary>Счётчик попыток входа для защиты от подбора пароля</summary>
        int attempts = 0;

        /// <summary>Флаг блокировки формы после превышения лимита попыток</summary>
        private bool isBlocked = false;

        /// <summary>Флаг необходимости ввода капчи</summary>
        private bool captchaRequired = false;

        /// <summary>Таймер для отсчёта времени блокировки</summary>
        private System.Windows.Threading.DispatcherTimer blockTimer;

        /// <summary>
        /// Конструктор страницы авторизации.
        /// Инициализирует компоненты, сервисы и сбрасывает форму.
        /// </summary>
        public Autho()
        {
            InitializeComponent();
            passwordRecoveryService = new PasswordRecoveryService();
            codeGenerator = new CodeGenerator();
            InitializeTimer();
            ResetForm();
        }

        /// <summary>
        /// Инициализирует таймер блокировки формы.
        /// Устанавливает интервал 1 секунду и подписывается на событие Tick.
        /// </summary>
        private void InitializeTimer()
        {
            blockTimer = new System.Windows.Threading.DispatcherTimer();
            blockTimer.Interval = TimeSpan.FromSeconds(1);
            blockTimer.Tick += BlockTimer_Tick;
        }

        /// <summary>
        /// Обработчик нажатия кнопки «Войти как гость».
        /// Выполняет навигацию на страницу клиента с ролью «Гость»,
        /// если форма не заблокирована.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void btnEnterGuest_Click(object sender, RoutedEventArgs e)
        {
            if (!isBlocked)
                NavigationService.Navigate(new Client(null, "Гость"));
        }

        /// <summary>
        /// Обработчик нажатия кнопки «Войти».
        /// Выполняет валидацию данных, проверку капчи, аутентификацию пользователя
        /// и двухфакторную проверку при необходимости.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            // Если форма заблокирована — выходим без действий
            if (isBlocked) return;

            /* 
             * Создаём модель валидации с данными из полей ввода.
             * Trim() удаляет пробелы по краям для корректной проверки.
             */
            var model = new AuthValidationModel
            {
                Email = tbLogin.Text?.Trim(),
                Password = tbPassword.Password?.Trim(),
                Captcha = tbCaptcha.Text?.Trim(),
                ExpectedCaptcha = tblCaptcha.Text?.Replace(" ", ""),
                CaptchaRequired = captchaRequired
            };

            // Выполняем валидацию данных через AuthValidator
            var validator = new AuthValidator();
            var result = validator.Validate(model);

            // Если валидация не пройдена — показываем ошибки и выходим
            if (!result.IsValid)
            {
                MessageBox.Show($"Ошибки:\n{result.ErrorMessage}",
                               "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isBlocked) return;

            // Проверка капчи, если она требуется
            if (captchaRequired)
            {
                if (string.IsNullOrWhiteSpace(tbCaptcha.Text) || tbCaptcha.Text != tblCaptcha.Text.Replace(" ", ""))
                {
                    MessageBox.Show("Неверная капча!");
                    ShowCaptcha();
                    return;
                }
            }

            // Увеличиваем счётчик попыток и получаем данные для входа
            attempts++;
            var login = tbLogin.Text.Trim();
            var password = tbPassword.Password.Trim();

            // Проверка на пустые поля
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
                    // Поиск пользователя в базе по логину и паролю
                    var user = db.Users.FirstOrDefault(x => x.Email == login && x.Password == password);

                    if (user != null)
                    {
                        // Проверка рабочего времени для сотрудников
                        if (!TimeService.IsWithinWorkingHours() && TimeService.IsEmployee(user))
                        {
                            MessageBox.Show("Доступ разрешен только в рабочее время (10:00-19:00)!");
                            return;
                        }

                        currentUserFor2FA = user;

                        /* 
                         * Если двухфакторная аутентификация не отключена пользователем —
                         * выполняем 2FA, иначе сразу авторизуем.
                         */
                        if (!cbDisable2FA.IsChecked.Value)
                        {
                            PerformTwoFactorAuthentication(user);
                        }
                        else
                        {
                            LoginSuccess(user);
                        }

                        // Сброс счётчика попыток и капчи после успешного входа
                        attempts = 0;
                        captchaRequired = false;
                        HideCaptcha();
                    }
                    else
                    {
                        // Обработка неверных данных входа
                        ShowErrorAndCaptcha();

                        // Блокировка формы после 4 неудачных попыток
                        if (attempts >= 4)
                        {
                            BlockForm(10);
                        }
                    }
                }
            }
            catch
            {
                // При ошибке базы данных — переход в гостевой режим
                if (!isBlocked)
                    NavigationService.Navigate(new Client(null, "Гость"));
            }
        }

        /// <summary>
        /// Выполняет двухфакторную аутентификацию пользователя.
        /// Генерирует код, отправляет его на email и проверяет ввод.
        /// </summary>
        /// <param name="user">Пользователь, проходящий аутентификацию</param>
        private void PerformTwoFactorAuthentication(Users user)
        {
            // Получаем email пользователя для отправки кода
            string email = user.Email;

            // Генерируем новый 4-значный код 2FA
            current2FACode = codeGenerator.GenerateCode();

            var emailService = new EmailService();

            // Отправляем код 2FA на email пользователя
            bool codeSent = emailService.SendEmail(
                email,
                "Код двухфакторной аутентификации",
                $"Ваш код для входа: {current2FACode}\n" +
                $"Код действителен в течение текущей сессии."
            );

            // Проверяем результат отправки письма
            if (!codeSent)
            {
                MessageBox.Show("Не удалось отправить код аутентификации. Попробуйте ещё раз.",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
                return;
            }

            // Создаем и показываем диалог ввода кода 2FA
            var twoFactorDialog = new TwoFactorDialog();

            // Проверяем, что пользователь ввел код
            if (twoFactorDialog.ShowDialog() == true)
            {
                // Получаем введенный код
                string enteredCode = twoFactorDialog.EnteredCode;

                // Проверяем правильность кода (без учета регистра)
                if (current2FACode.Equals(enteredCode, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Код подтверждён!",
                                   "Успех",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                    // Выполняем успешный вход пользователя
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

        /// <summary>
        /// Выполняет успешный вход пользователя в систему.
        /// Определяет роль и выполняет навигацию на главную страницу.
        /// </summary>
        /// <param name="user">Авторизованный пользователь</param>
        private void LoginSuccess(Users user)
        {
            var role = GetRole(user);
            MessageBox.Show($"Вы вошли как: {role}");
            NavigationService.Navigate(new Client(user, role));
        }

        /// <summary>
        /// Показывает ошибку входа и активирует капчу.
        /// Очищает поле пароля для повторного ввода.
        /// </summary>
        private void ShowErrorAndCaptcha()
        {
            MessageBox.Show("Неверный логин или пароль!");

            captchaRequired = true;
            ShowCaptcha();
            tbPassword.Clear();
        }

        /// <summary>
        /// Отображает капчу: делает поля видимыми и генерирует новое значение.
        /// </summary>
        private void ShowCaptcha()
        {
            tbCaptcha.Visibility = Visibility.Visible;
            tblCaptcha.Visibility = Visibility.Visible;
            tblCaptcha.Text = SimpleCaptcha.Create();
            tblCaptcha.TextDecorations = TextDecorations.Strikethrough;
            tbCaptcha.Clear();
        }

        /// <summary>
        /// Скрывает поля капчи и сбрасывает флаг необходимости ввода.
        /// </summary>
        private void HideCaptcha()
        {
            tbCaptcha.Visibility = Visibility.Hidden;
            tblCaptcha.Visibility = Visibility.Hidden;
            captchaRequired = false;
        }

        /// <summary>
        /// Определяет роль пользователя на основе должности или email.
        /// </summary>
        /// <param name="user">Пользователь для определения роли</param>
        /// <returns>Строковое название роли</returns>
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

            // Резервная проверка по email, если не удалось определить через БД
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

        /// <summary>
        /// Сбрасывает форму авторизации к начальному состоянию.
        /// Очищает поля ввода и скрывает капчу.
        /// </summary>
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

        /// <summary>
        /// Блокирует форму на указанное время в секундах.
        /// Отключает элементы управления и запускает таймер.
        /// </summary>
        /// <param name="seconds">Время блокировки в секундах</param>
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

        /// <summary>Оставшееся время блокировки в секундах</summary>
        private int remainingBlockTime = 0;

        /// <summary>
        /// Обработчик тика таймера блокировки.
        /// Уменьшает счётчик и разблокирует форму при достижении нуля.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            remainingBlockTime--;
            UpdateTimerText();

            if (remainingBlockTime <= 0)
            {
                UnblockForm();
            }
        }

        /// <summary>
        /// Обновляет текст таймера блокировки в интерфейсе.
        /// </summary>
        private void UpdateTimerText()
        {
            tbTimer.Text = $"До разблокировки осталось: {remainingBlockTime} сек.";
        }

        /// <summary>
        /// Разблокирует форму после истечения времени блокировки.
        /// Сбрасывает флаги и восстанавливает элементы управления.
        /// </summary>
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

        /// <summary>
        /// Обработчик нажатия кнопки «Забыли пароль?».
        /// Запускает процесс восстановления пароля через email.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void btnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            // Создаем и показываем диалог ввода email
            var recoveryDialog = new PasswordRecoveryDialog();

            // Проверяем, что пользователь подтвердил ввод email
            if (recoveryDialog.ShowDialog() == true)
            {
                // Получаем введенный email
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

                // Отправляем код восстановления на указанный email
                bool codeSent = passwordRecoveryService.SendRecoveryCode(email);
                if (!codeSent)
                {
                    MessageBox.Show("Не удалось отправить код на указанный email",
                                   "Ошибка",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Error);
                    return;
                }

                // Создаем и показываем диалог ввода кода подтверждения
                var codeDialog = new CodeVerificationDialog();

                // Проверяем, что пользователь ввел код
                if (codeDialog.ShowDialog() == true)
                {
                    // Получаем введенный код
                    string enteredCode = codeDialog.EnteredCode;

                    // Проверяем правильность кода
                    if (passwordRecoveryService.VerifyCode(enteredCode))
                    {
                        // Создаем и показываем диалог ввода нового пароля
                        var resetDialog = new ResetPasswordDialog();

                        // Проверяем, что пользователь подтвердил новый пароль
                        if (resetDialog.ShowDialog() == true)
                        {
                            // Получаем новый пароль
                            string newPassword = resetDialog.NewPassword;

                            // Сбрасываем пароль в базе данных
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