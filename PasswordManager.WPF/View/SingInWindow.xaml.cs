using PasswordManager.Core;
using System.Windows;
using System.Windows.Input;

namespace PasswordManager.WPF
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class SignInWindow : Window
    {
        public SignInWindow()
        {
            InitializeComponent();

            EntryPassword.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter) ButtonOkClick(null, null);
            };
            EntryPassword.Focus();
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            if (MainProcess.CheckMasterPassword(EntryPassword.Password))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                ErrorLabel.Visibility = Visibility.Visible;
                EntryPassword.Clear();
                EntryPassword.Focus();
            }
        }
    }
}
