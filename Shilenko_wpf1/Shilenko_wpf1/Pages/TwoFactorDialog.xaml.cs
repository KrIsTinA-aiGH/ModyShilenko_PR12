using System.Windows;

namespace Shilenko_wpf1.Pages
{
    public partial class TwoFactorDialog : Window
    {
        ///введенный пользователем код 2FA
        public string EnteredCode { get; private set; }

        public TwoFactorDialog()
        {
            InitializeComponent();
        }

        ///обработка нажатия кнопки "Подтвердить"
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
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

            ///Сохраняем введенный код в свойство
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