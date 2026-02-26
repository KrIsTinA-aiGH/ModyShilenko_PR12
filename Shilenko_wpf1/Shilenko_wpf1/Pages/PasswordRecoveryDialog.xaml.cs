using System.Windows;

namespace Shilenko_wpf1.Pages
{
    public partial class PasswordRecoveryDialog : Window
    {
        
        public string EnteredEmail { get; private set; }

        public PasswordRecoveryDialog()
        {
            InitializeComponent();
        }

        ///обработка нажатия кнопки "Отправить код"
        private void btnSendCode_Click(object sender, RoutedEventArgs e)
        {
            ///Получаем введенный email и удаляем пробелы по краям
            string email = tbEmail.Text?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Пожалуйста, введите email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                MessageBox.Show("Некорректный формат email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EnteredEmail = email;
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