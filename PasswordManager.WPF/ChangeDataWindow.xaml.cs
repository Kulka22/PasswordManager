using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PasswordManager.WPF
{
    public partial class ChangeDataWindow : Window
    {
        private string _service;
        private string _url;
        private string _category;
        private string _newLogin;
        private string _newPassword;

        public string Service
        {
            get => _service;
            set => _service = value;
        }
        public string URL
        {
            get => _url;
            set => _url = value;
        }
        public string Category
        {
            get => _category;
            set => _category = value;
        }
        public string NewLogin 
        { 
            get => _newLogin;
            set => _newLogin = value;
        }
        public string NewPassword
        {
            get => _newPassword;
            set => _newPassword = value; 
        }
        public ChangeDataWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
