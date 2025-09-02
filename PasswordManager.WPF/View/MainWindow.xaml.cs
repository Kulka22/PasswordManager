using PasswordManager.Core;
using PasswordManager.WPF.Models;
using System.Windows;

namespace PasswordManager.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel mainVM)
        {
            InitializeComponent();
            DataContext = mainVM;
        }
    }
}