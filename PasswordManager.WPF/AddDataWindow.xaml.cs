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
using PasswordManager.Core;

namespace PasswordManager.WPF
{

    public partial class AddDataWindow : Window
    {
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
        public AddDataWindow()
        {
            InitializeComponent();
        }

        private void GeneratePassword(object sender, RoutedEventArgs e)
        {
            PasswordText.Text = MainProcess.GeneratePassword(10, forbiddenSymbols);
        }

        private void HideError(object sender, RoutedEventArgs e)
        {
            if (errorSULabel.Visibility == Visibility.Visible)
            {
                errorSULabel.Visibility = Visibility.Collapsed;
            }
            if (errorLPLabel.Visibility == Visibility.Visible)
            {
                errorLPLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            if (UrlText.Text == "" && ServiceText.Text == "")
            {
                errorSULabel.Visibility = Visibility.Visible;
            }
            if (LoginText.Text == "" && PasswordText.Text == "")
            {
                errorLPLabel.Visibility = Visibility.Visible;
            }

            if (errorSULabel.Visibility == Visibility.Collapsed && errorLPLabel.Visibility == Visibility.Collapsed)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
