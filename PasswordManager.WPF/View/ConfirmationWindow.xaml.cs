using System.Windows;

namespace PasswordManager.WPF
{
    public partial class ConfirmationWindow : Window
    {
        private string _message;

        public string Message
        {
            get => _message;
            set => _message = value;
        }

        public ConfirmationWindow()
        {
            InitializeComponent();
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonNegativeClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
