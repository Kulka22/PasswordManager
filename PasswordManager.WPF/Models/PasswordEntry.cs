using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PasswordManager.WPF.Models
{
    public class PasswordEntry : INotifyPropertyChanged
    {
        private string _id, _service, _category, _url, _login, _password;
        private bool _isVisible = false;

        public string ID
        {
            get => _id;
            set => _id = value;
        }
        public string Service { get => _service; set => _service = value; }
        public string Category { get => _category; set => _category = value; }
        public string Url { get => _url; set => _url = value; }

        public string Login
        {
            get => _login;
            set => _login = value;
        }
        public string DisplayLogin
        {
            get => _isVisible ? _login : new string('*', _login?.Length ?? 0);
        }
        

        public string Password
        {
            get => _password;
            set => _password = value;
        }
        public string DisplayPassword
        {
            get => _isVisible ? _password : new string ('*', _password?.Length ?? 0);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayLogin));
                OnPropertyChanged(nameof(DisplayPassword));
            }
        }

        public ICommand ToggleVisibilityCommand => new RelayCommand(ToggleVisibility);

        private void ToggleVisibility()
        {
            IsVisible = !IsVisible;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}