using System.Windows;

namespace Shilenko_wpf1.Pages
{
    public partial class CodeVerificationDialog : Window
    {
        ///введенный пользователем код подтверждения
        public string EnteredCode { get; private set; }

        public CodeVerificationDialog()
        {
            InitializeComponent();
        }

        ///обработка нажатия кнопки "Проверить"
        private void btnVerify_Click(object sender, RoutedEventArgs e)
        {
            string code = tbCode.Text?.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Пожалуйста, введите код", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (code.Length != 4 || !int.TryParse(code, out _))
            {
                MessageBox.Show("Код должен быть 4-значным числом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EnteredCode = code;
            DialogResult = true;
            Close();
        }

        ///обработка нажатия кнопки "Отмена"
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}