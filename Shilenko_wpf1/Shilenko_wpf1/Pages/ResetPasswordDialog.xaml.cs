using System.Windows;

namespace Shilenko_wpf1.Pages
{
    /// <summary>
    /// Диалоговое окно для сброса пароля.
    /// Позволяет пользователю ввести новый пароль и подтвердить его.
    /// </summary>
    public partial class ResetPasswordDialog : Window
    {
        /// <summary>
        /// Новый пароль, введенный пользователем (доступен только для чтения извне).
        /// </summary>
        public string NewPassword { get; private set; }

        /// <summary>
        /// Конструктор окна сброса пароля.
        /// Инициализирует компоненты интерфейса.
        /// </summary>
        public ResetPasswordDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик нажатия кнопки «Сохранить».
        /// Выполняет валидацию пароля и закрывает окно с результатом.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Получаем новый пароль из первого поля
            string newPassword = pbNewPassword.Password?.Trim();

            // Получаем подтверждение пароля из второго поля
            string confirmPassword = pbConfirmPassword.Password?.Trim();

            // Проверка: оба поля должны быть заполнены
            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            /* 
             * Проверка сложности пароля:
             * - Минимальная длина: 6 символов
             * - Рекомендуется использовать буквы и цифры
             */
            if (newPassword.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать не менее 6 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка: пароли должны совпадать
            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Сохраняем новый пароль в свойство
            NewPassword = newPassword;

            // Устанавливаем DialogResult = true для обозначения успеха
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Обработчик нажатия кнопки «Отмена».
        /// Закрывает окно с результатом «отменено».
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Устанавливаем DialogResult = false для обозначения отмены
            DialogResult = false;
            Close();
        }
    }
}