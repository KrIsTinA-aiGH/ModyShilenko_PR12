using System.Windows;

namespace Shilenko_wpf1.Pages
{
    /// <summary>
    /// Диалоговое окно для двухфакторной аутентификации.
    /// Запрашивает у пользователя 4-значный код, отправленный на email.
    /// </summary>
    public partial class TwoFactorDialog : Window
    {
        /// <summary>
        /// Введенный пользователем код 2FA (доступен только для чтения извне).
        /// </summary>
        public string EnteredCode { get; private set; }

        /// <summary>
        /// Конструктор окна двухфакторной аутентификации.
        /// Инициализирует компоненты интерфейса.
        /// </summary>
        public TwoFactorDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик нажатия кнопки «Подтвердить».
        /// Выполняет валидацию введенного кода и закрывает окно с результатом.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // Получаем код из поля ввода и удаляем пробелы по краям
            string code = tbCode.Text?.Trim();

            // Проверка: код не должен быть пустым
            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Пожалуйста, введите код", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            /* 
             * Проверка формата кода:
             * - Должен быть ровно 4 символа
             * - Должен состоять только из цифр
             */
            if (code.Length != 4 || !int.TryParse(code, out _))
            {
                MessageBox.Show("Код должен быть 4-значным числом", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Сохраняем введенный код в свойство
            EnteredCode = code;

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