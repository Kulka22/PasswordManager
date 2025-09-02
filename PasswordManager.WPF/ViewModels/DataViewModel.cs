using PasswordManager.Core;
using PasswordManager.WPF.Models;
using PasswordManager.WPF.Services;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PasswordManager.WPF.ViewModels
{
    public class DataViewModel : INotifyPropertyChanged, INotifyDataErrorInfo, ICloseable
    {
        private readonly Dictionary<string, List<string>> _errors = new();

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<CloseRequestedEventArgs> CloseRequested;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public bool HasErrors => _errors.Any();
        public IEnumerable GetErrors(string? propertyName)
        {
            Console.WriteLine($"GetErrors called for {propertyName}");

            if (string.IsNullOrEmpty(propertyName))
                return _errors.Values.SelectMany(errors => errors);

            return _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();
        }

        protected void OnErrorsChanged(string propertyName) =>
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

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
                _password = value;
                OnPropertyChanged();
            }
        }

        public ICommand GeneratePasswordCommand => new RelayCommand(GeneratePassword);
        public ICommand ButtonOkCommand => new RelayCommand(ButtonOkClick);
        

        private void GeneratePassword()
        {
            string newPassword = MainProcess.GeneratePassword(10, forbiddenSymbols);
            Password = newPassword;
        }

        private void ButtonOkClick()
        {
            bool checkErrors = true;
            if (CheckSevice() && CheckUrl())
            {
                checkErrors = false;
                AddError(nameof(Service), "Хотя бы одно поле из Servise или Url должно быть заполнено");
                AddError(nameof(Url), "Хотя бы одно поле из Servise или Url должно быть заполнено");
            }
            if (CheckLogin() && CheckPassword())
            {
                checkErrors = false;
                AddError(nameof(Login), "Хотя бы одно поле из Login или Password должно быть заполнено");
                AddError(nameof(Password), "Хотя бы одно поле из Login или Password должно быть заполнено");
            }
            if (checkErrors)
            {
                CloseRequested?.Invoke(this, new CloseRequestedEventArgs { DialogResult = true });
            }
        }

        private bool CheckSevice()
        {
            ClearErrors(nameof(Service));

            return string.IsNullOrEmpty(Service);
        }
        private bool CheckUrl()
        {
            ClearErrors(nameof(Url));

            return string.IsNullOrEmpty(Url);
        }
        private bool CheckLogin()
        {
            ClearErrors(nameof(Login));

            return string.IsNullOrEmpty(Login);
        }
        private bool CheckPassword()
        {
            ClearErrors(nameof(Password));

            return string.IsNullOrEmpty(Password);
        }



        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();

            if (!_errors[propertyName].Contains(error))
                _errors[propertyName].Add(error);

            OnErrorsChanged(propertyName);
        }
        private void ClearErrors(string propertyName)
        {
            if (_errors.Remove(propertyName))
                OnErrorsChanged(propertyName);
        }
    }
}
