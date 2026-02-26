using System.Windows;

namespace Shilenko_wpf1.Pages
{
    /// <summary>
    /// Диалоговое окно для восстановления пароля.
    /// Запрашивает email пользователя для отправки кода сброса.
    /// </summary>
    public partial class PasswordRecoveryDialog : Window
    {
        /// <summary>
        /// Введенный пользователем email (доступен только для чтения извне).
        /// </summary>
        public string EnteredEmail { get; private set; }

        /// <summary>
        /// Конструктор окна восстановления пароля.
        /// Инициализирует компоненты интерфейса.
        /// </summary>
        public PasswordRecoveryDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик нажатия кнопки «Отправить код».
        /// Выполняет валидацию email и закрывает окно с результатом.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void btnSendCode_Click(object sender, RoutedEventArgs e)
        {
            // Получаем введенный email и удаляем пробелы по краям
            string email = tbEmail.Text?.Trim();

            // Проверка: email не должен быть пустым
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Пожалуйста, введите email", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            /* 
             * Базовая проверка формата email:
             * - Должен содержать символ @
             * - Должен содержать точку (домен)
             * Примечание: это упрощённая проверка, для продакшена рекомендуется использовать Regex
             */
            if (!email.Contains("@") || !email.Contains("."))
            {
                MessageBox.Show("Некорректный формат email", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Сохраняем email и закрываем окно с успешным результатом
            EnteredEmail = email;
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