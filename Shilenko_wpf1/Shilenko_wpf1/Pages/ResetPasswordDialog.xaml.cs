using System.Windows;

namespace Shilenko_wpf1.Pages
{
    public partial class ResetPasswordDialog : Window
    {
        ///новый пароль, введенный пользователем
        public string NewPassword { get; private set; }

        public ResetPasswordDialog()
        {
            InitializeComponent();
        }

        ///обработка нажатия кнопки "Сохранить"
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = pbNewPassword.Password?.Trim();
            ///Получаем подтверждение пароля из второго поля
            string confirmPassword = pbConfirmPassword.Password?.Trim();


            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать не менее 6 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ///Сохраняем новый пароль в свойство
            NewPassword = newPassword;
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