using System.Windows;

namespace Shilenko_wpf1.Pages
{
    /// <summary>
    /// Диалоговое окно для подтверждения кода двухфакторной аутентификации
    /// или восстановления пароля.
    /// </summary>
    public partial class CodeVerificationDialog : Window
    {
        /// <summary>
        /// Введенный пользователем код (доступен только для чтения извне).
        /// </summary>
        public string EnteredCode { get; private set; }

        /// <summary>
        /// Конструктор окна подтверждения кода.
        /// Инициализирует компоненты интерфейса.
        /// </summary>
        public CodeVerificationDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик нажатия кнопки «Проверить».
        /// Выполняет валидацию введенного кода и закрывает окно с результатом.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void btnVerify_Click(object sender, RoutedEventArgs e)
        {
            // Получаем код из поля ввода, удаляем пробелы по краям
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

            // Сохраняем код и закрываем окно с успешным результатом
            EnteredCode = code;
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