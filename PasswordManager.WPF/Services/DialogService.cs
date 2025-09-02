using System;
using System.Collections.Generic;
using System.Windows;

namespace PasswordManager.WPF.Services
{
    public class DialogService : IDialogService
    {
        private readonly Dictionary<Type, Type> _mappings = new();
        private readonly IServiceProvider _serviceProvider;

        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void RegisterWindow<TViewModel, TWindow>() where TWindow : Window
        {
            _mappings[typeof(TViewModel)] = typeof(TWindow);
        }

        public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class
        {

            if (!_mappings.TryGetValue(typeof(TViewModel), out var windowType))
                throw new InvalidOperationException($"No window registered for {typeof(TViewModel)}");

            var window = (Window)Activator.CreateInstance(windowType)!;
            window.DataContext = viewModel;

            if (viewModel is ICloseable closeable)
            {
                closeable.CloseRequested += (sender, args) =>
                {
                    window.DialogResult = args.DialogResult;
                    window.Close();
                };
            }

            return window.ShowDialog();
        }

        // Аналогично для ShowWindow
        public void ShowWindow<TViewModel>(Action<TViewModel> setup = null) where TViewModel : class
        {
            var viewModel = (TViewModel)_serviceProvider.GetService(typeof(TViewModel));
            setup?.Invoke(viewModel);

            if (!_mappings.TryGetValue(typeof(TViewModel), out var windowType))
                throw new InvalidOperationException($"No window registered for {typeof(TViewModel)}");
            
            var window = (Window)Activator.CreateInstance(windowType)!;
            window.DataContext = viewModel;

            window.Show();
        }
    }
}
