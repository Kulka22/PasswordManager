using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PasswordManager.WPF.Models
{
    public class PasswordEntry
    {

        private string _servise;
        private string _category;
        private string _url;
        private string _login;
        private string _password;
        private bool _isVisible = true;

        public string Service
        {
            get => _servise;
            set => _servise = value;
        }
        public string Category
        {
            get => _category;
            set => _category = value;
        }
        public string Url
        {
            get => _url;
            set => _url = value;
        }
        public string Login
        {
            get
            {
                if (_isVisible) return _login;
                else return new string('*', _login?.Length ?? 0);
            }
                //_isVisible ? _login : new string('*', _login?.Length ?? 0);
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }
        public string Password
        {
            get => _isVisible ? _password : new string('*', _password?.Length ?? 0);
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
            }
        }

        public ICommand ToggleVisibilityCommand => new RelayCommand(ToggleVisibility);

        private void ToggleVisibility()
        {
            IsVisible = !IsVisible;
            OnPropertyChanged(nameof(Login));
            OnPropertyChanged(nameof(Password));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    } 
}