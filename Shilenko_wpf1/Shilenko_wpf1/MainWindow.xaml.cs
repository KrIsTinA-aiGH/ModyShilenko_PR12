using Shilenko_wpf1.Pages;
using System;
using System.Windows;

namespace Shilenko_wpf1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            fr_content.Content = new Autho();
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (fr_content.CanGoBack)
                fr_content.GoBack();
        }

        private void frame_Content(object sender, EventArgs e)
        {
            if (fr_content.CanGoBack)
                btnBack.Visibility = Visibility.Visible;
            else
                btnBack.Visibility = Visibility.Hidden;
        }
    }
}