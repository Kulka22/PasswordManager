using PasswordManager.Core;
using PasswordManager.WPF.Models;
using System.Windows;

namespace PasswordManager.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var mainProcess = InitializateMainProcess();
            if (mainProcess != null) DataContext = new MainViewModel(mainProcess);
            else
            {
                Close();
                return;
            }
        }
        private MainProcess? InitializateMainProcess()
        {
            if (!MainProcess.GetRegStatus())
            {
                RegistrationWindow registrationWindow = new RegistrationWindow();
                if (registrationWindow.ShowDialog() == true)
                {
                    return new MainProcess(registrationWindow.EntryPassword.Text);
                }
                return null;
            }
            else
            {
                SignInWindow signInWindow = new SignInWindow();
                bool? result = signInWindow.ShowDialog();
                if (result == true)
                {
                    return new MainProcess(signInWindow.EntryPassword.Password);
                }
                return null;
            }
        }
    }
}