using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PasswordManager.WPF.Models
{
    public class PasswordEntry : INotifyPropertyChanged
    {
        private string _id, _service, _category, _url, _login, _password;
        private bool _isVisible = false;
        private bool _isToggling = false; // Флаг для блокировки рекурсии

        public string ID
        {
            get => _id;
            set => _id = value;
        }
        public string Service { get => _service; set => SetField(ref _service, value); }
        public string Category { get => _category; set => SetField(ref _category, value); }
        public string Url { get => _url; set => SetField(ref _url, value); }

        public string Login
        {
            get => _login;
            set => SetField(ref _login, value);
        }
        public string DisplayLogin
        {
            get => _isVisible ? _login : new string('*', _login?.Length ?? 0);
        }
        

        public string Password
        {
            get => _password;
            set => SetField(ref _password, value);
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

                _isToggling = true; // Блокируем рекурсию
                _isVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayLogin));
                OnPropertyChanged(nameof(DisplayPassword));
                _isToggling = false; // Разблокируем
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

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}