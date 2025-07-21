using PasswordManager.Core;
using PasswordManager.WPF.Converters;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PasswordManager.WPF.Models
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
        public event EventHandler? CanExecuteChanged;
    }
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) =>
            _canExecute == null || (parameter is T typedParam && _canExecute(typedParam));

        public void Execute(object parameter)
        {
            if (parameter is T typedParam)
                _execute(typedParam);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly MainProcess _mainProcess;
        private ObservableCollection<PasswordEntry> _allPasswords;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<PasswordEntry> FilteredPasswords { get; } = new ObservableCollection<PasswordEntry>();
        public ObservableCollection<CategoryView> Categories { get; } = new ObservableCollection<CategoryView>();

        public ICommand AddCommand => new RelayCommand(AddPassword);
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        
        public MainViewModel(MainProcess mainProcess)
        {
            _mainProcess = mainProcess;
            _allPasswords = new ObservableCollection<PasswordEntry>(_mainProcess.GetPasswords().Select(PasswordEntryConverter.ToLocal).ToList());

            foreach (string category in _mainProcess.GetAllCategories())
            {
                var vm = new CategoryView(category);
                vm.SelectionChanged += OnCategorySelectionChanged;
                Categories.Add(vm);
            }

            UpdateFilter();

            EditCommand = new RelayCommand<PasswordEntry>(EditPassword);
            DeleteCommand = new RelayCommand<PasswordEntry>(DeletePassword);
        }

        private void OnCategorySelectionChanged(object sender, EventArgs e)
        {
            UpdateFilter();
        }

        private void UpdateFilter()
        {
            var selectedCategories = Categories
                .Where(c => c.IsSelected)
                .Select(c => c.Name)
                .ToList();

            FilteredPasswords.Clear();

            if (selectedCategories.Count == 0)
            {
                foreach (var item in _allPasswords)
                {
                    FilteredPasswords.Add(item);
                }
            }
            else
            {
                foreach (var item in _allPasswords.Where(p => selectedCategories.Contains(p.Category)))
                {
                    FilteredPasswords.Add(item);
                }
            }
        }

        private void UpdatePasswords()
        {
            _allPasswords.Clear();
            
            foreach (var item in _mainProcess.GetPasswords().Select(PasswordEntryConverter.ToLocal))
            {
                _allPasswords.Add(item);
            }

            UpdateFilter();
        }

        private void AddPassword()
        {
            DataWindow addDataWindow = new DataWindow
            {
                Title = "Добавить запись",
            };

            if (addDataWindow.ShowDialog() == true)
            {
                PasswordEntry newItem = new PasswordEntry();
                newItem.Service = addDataWindow.Service;
                newItem.Category = addDataWindow.Category;
                newItem.Url = addDataWindow.Url;
                newItem.Login = addDataWindow.Login;
                newItem.Password = addDataWindow.Password;

                var tuple = _mainProcess.FindRepetition(PasswordEntryConverter.ToExternal(newItem));
                if (tuple != null)
                {
                    if (tuple.Value.Item2)
                    {
                        MessageWindow messageWindow = new MessageWindow();
                        messageWindow.Message = "Такая запись уже существует!";
                        messageWindow.Show();
                    }
                    else
                    {
                        ConfirmationWindow confirmationWindow = new ConfirmationWindow();
                        confirmationWindow.Message = "Существует запись с таким же Service и Login.\nИзменить пароль в уже существующей записи?";
                        if (confirmationWindow.ShowDialog() == true)
                        {
                            tuple.Value.Item1.Password = newItem.Password;
                            _mainProcess.ChangePassword(tuple.Value.Item1);
                            _mainProcess.SavePasswords();
                            UpdatePasswords();
                        }
                    }
                }
                else
                {
                    _mainProcess.AddPassword(PasswordEntryConverter.ToExternal(newItem));
                    _mainProcess.SavePasswords();
                    UpdatePasswords();
                }
            }
        }

        private void EditPassword(PasswordEntry item)
        {
            var changeDataWindow = new DataWindow
            {
                Title = "Изменить запись",
                Service = item.Service,
                Url = item.Url,
                Category = item.Category,
                Login = item.Login,
                Password = item.Password
            };

            if (changeDataWindow.ShowDialog() != true) return;
            
            item.Service = changeDataWindow.Service;
            item.Url = changeDataWindow.Url;
            item.Category = changeDataWindow.Category;
            item.Login = changeDataWindow.Login;
            item.Password = changeDataWindow.Password;

            _mainProcess.ChangePassword(PasswordEntryConverter.ToExternal(item));
            _mainProcess.SavePasswords();
            UpdatePasswords();
        }

        private void DeletePassword(PasswordEntry item)
        {
            ConfirmationWindow confirmationWindow = new ConfirmationWindow();
            if (confirmationWindow.ShowDialog() == true)
            {
                _mainProcess.RemovePassword(PasswordEntryConverter.ToExternal(item));
                _mainProcess.SavePasswords();
            }
        }
    }
}