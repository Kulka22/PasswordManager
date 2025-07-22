using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
using PasswordManager.Core;
using PasswordManager.WPF.Models;

namespace PasswordManager.WPF
{

    public partial class DataWindow : Window
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        List<char> forbiddenSymbols = new List<char>
            {
                '\0',       // Нулевой символ (ASCII 0)
                '/',        // Слэш (/)
                '\\',       // Обратный слэш (\) - экранируется как '\\'
                '\"',       // Двойные кавычки (") - экранируется как '\"'
                '\'',       // Одинарные кавычки (') - экранируется как '\''
                '|',        // Вертикальная черта (|)
                '^',        // Карет (^)
                '[',        // Открывающая квадратная скобка ([)
                ']',        // Закрывающая квадратная скобка (])
                '{',        // Открывающая фигурная скобка ({)
                '}',        // Закрывающая фигурная скобка (})
                '<',        // Открывающая угловая скобка (<)
                '>',        // Закрывающая угловая скобка (>)
                '+',        // Плюс (+)
                '='         // Знак равенства (=)
            };

        private string _servise;
        private string _category;
        private string _url;
        private string _login;
        private string _password;
        public string Service
        {
            get => _servise;
            set
            {
                _servise = value;
            }
        }
        public string Category
        {
            get => _category;
            set
            {
                _category = value;
            }
        }
        public string Url
        {
            get => _url;
            set
            {
                _url = value;
            }
        }
        public string Login
        {
            get => _login;
            set
            {
                _login = value;
            }
        }
        public string Password
        {
            get => _password;
            set
            {
                Debug.WriteLine(_password);
                _password = value;
                OnPropertyChanged(nameof(Password));
                Debug.WriteLine(_password);
            }
        }

        public ICommand GeneratePasswordCommand => new RelayCommand(GeneratePassword);
        public DataWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void GeneratePassword()
        {
            Debug.WriteLine(Password);
            Password = MainProcess.GeneratePassword(10, forbiddenSymbols);
            Debug.WriteLine(Password);
        }

        private void HideError(object sender, RoutedEventArgs e)
        {
            if (errorSULabel.Visibility == Visibility.Visible)
            {
                errorSULabel.Visibility = Visibility.Hidden;
            }
            if (errorLPLabel.Visibility == Visibility.Visible)
            {
                errorLPLabel.Visibility = Visibility.Hidden;
            }
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            if (Url == "" && Service == "")
            {
                errorSULabel.Visibility = Visibility.Visible;
            }
            if (Login == "" && Password == "")
            {
                errorLPLabel.Visibility = Visibility.Visible;
            }

            if (errorSULabel.Visibility == Visibility.Hidden && errorLPLabel.Visibility == Visibility.Hidden)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
