using Shilenko_wpf1.Pages;
using System;
using System.Windows;

namespace Shilenko_wpf1
{
    /// <summary>
    /// Главное окно приложения «Автобаза».
    /// Содержит фрейм для навигации между страницами и кнопку «Назад».
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Конструктор главного окна.
        /// Инициализирует компоненты и загружает страницу авторизации.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Устанавливаем страницу авторизации как начальную
            fr_content.Content = new Autho();
        }

        /// <summary>
        /// Обработчик нажатия кнопки «Назад».
        /// Выполняет навигацию на предыдущую страницу во фрейме.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем возможность возврата назад
            if (fr_content.CanGoBack)
                fr_content.GoBack();
        }

        /// <summary>
        /// Обработчик события завершения загрузки страницы во фрейме.
        /// Управляет видимостью кнопки «Назад» в зависимости от истории навигации.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void frame_Content(object sender, EventArgs e)
        {
            /* 
             * Показываем кнопку «Назад» только если есть страницы в истории:
             * - CanGoBack = true → кнопка видима
             * - CanGoBack = false → кнопка скрыта
             */
            if (fr_content.CanGoBack)
                btnBack.Visibility = Visibility.Visible;
            else
                btnBack.Visibility = Visibility.Hidden;
        }
    }
}