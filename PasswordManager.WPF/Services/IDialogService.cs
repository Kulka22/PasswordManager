using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordManager.WPF.Services
{
    public interface IDialogService
    {
        public void RegisterWindow<TViewModel, TWindow>() where TWindow : Window;
        void ShowWindow<TViewModel>(Action<TViewModel> setup = null) where TViewModel : class;
        bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : class;
    }
}
