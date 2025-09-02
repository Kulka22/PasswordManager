using PasswordManager.WPF.Services;
using System.Windows;
using PasswordManager.WPF.ViewModels;
using PasswordManager.WPF.Models;
using PasswordManager.Core;
using Microsoft.Extensions.DependencyInjection;


namespace PasswordManager.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var services = new ServiceCollection();

            services.AddSingleton<IDialogService, DialogService>();
            services.AddTransient<DataViewModel>();
            services.AddTransient<MainViewModel>();

            var serviceProvider = services.BuildServiceProvider();

            var dialogService = serviceProvider.GetService<IDialogService>();
            dialogService.RegisterWindow<DataViewModel, DataWindow>();

            var mainProcess = InitializateMainProcess();
            if (mainProcess != null)
            {
                var mainVM = new MainViewModel(dialogService, mainProcess);

                // Создаем главное окно
                var mainWindow = new MainWindow(mainVM);
                mainWindow.Show();
                ShutdownMode = ShutdownMode.OnMainWindowClose;
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
