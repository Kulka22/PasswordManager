using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PasswordManager.WPF.Models
{
    public class CategoryView : INotifyPropertyChanged
    {
        private bool _isSelected;
        public string Name { get; }
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public EventHandler SelectionChanged;

        public CategoryView(string name)
        {
            Name = name;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
