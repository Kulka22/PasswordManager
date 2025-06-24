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
        public string NewLogin { get; private set; }
        public string NewPassword { get; private set; }
        public ChangeDataWindow()
        {
            InitializeComponent();

        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            NewPassword = Password.Text;
            DialogResult = true;
            Close();
        }
    }
}
