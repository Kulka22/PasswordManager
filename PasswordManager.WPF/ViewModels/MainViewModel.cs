using PasswordManager.Core;
using PasswordManager.WPF.Converters;
using PasswordManager.WPF.ViewModels;
using PasswordManager.WPF.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PasswordManager.WPF.Models
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        [DebuggerStepThrough]
        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null) return true;
            else
            {
                return _canExecute.Invoke(parameter);
            }
        }
        public void Execute(object? parameter) => _execute();
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
        [DebuggerStepThrough]
        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null) return true;
            else
            {
                return (parameter is T typedParam && _canExecute(typedParam));
            }
        }
        public void Execute(object? parameter)
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
        private readonly IDialogService _dialogService;
        private ObservableCollection<PasswordEntry> _allPasswords;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<PasswordEntry> FilteredPasswords { get; } = new ObservableCollection<PasswordEntry>();
        public ObservableCollection<CategoryModel> Categories { get; } = new ObservableCollection<CategoryModel>();

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        
        public MainViewModel(IDialogService dialogService, MainProcess mainProcess)
        {
            _mainProcess = mainProcess;
            _dialogService = dialogService;

            _allPasswords = new ObservableCollection<PasswordEntry>(_mainProcess.GetPasswords().Select(PasswordEntryConverter.ToLocal).ToList());

            foreach (string category in _mainProcess.GetAllCategories())
            {
                var vm = new CategoryModel(category);
                vm.SelectionChanged += OnCategorySelectionChanged;
                Categories.Add(vm);
            }

            UpdateFilter();

            AddCommand = new RelayCommand(AddPassword);
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
            var dataVM = new DataViewModel();

            string serviceFrom = "";
            string categoryFrom = "";
            string urlFrom = "";
            string loginFrom = "";
            string passwordFrom = "";

            bool? result = _dialogService.ShowDialog<DataViewModel>(dataVM);

            if (result == true)
            {
                serviceFrom = dataVM.Service;
                categoryFrom = dataVM.Category;
                urlFrom = dataVM.Url;
                loginFrom = dataVM.Login;
                passwordFrom = dataVM.Password;

                PasswordEntry newItem = new PasswordEntry();
                newItem.Service = serviceFrom;
                newItem.Category = categoryFrom;
                newItem.Url = urlFrom;
                newItem.Login = loginFrom;
                newItem.Password = passwordFrom;

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
            var dataVM = new DataViewModel();
            dataVM.Service = item.Service;
            dataVM.Category = item.Category;
            dataVM.Url = item.Url;
            dataVM.Login = item.Login;
            dataVM.Password = item.Password;

            string serviceFrom = "";
            string categoryFrom = "";
            string urlFrom = "";
            string loginFrom = "";
            string passwordFrom = "";

            bool? result = _dialogService.ShowDialog<DataViewModel>(dataVM);

            if (result == true)
            {
                if (item.Service == serviceFrom &&
                    item.Category == categoryFrom &&
                    item.Url == urlFrom &&
                    item.Login == loginFrom &&
                    item.Password == passwordFrom) return;

                item.Service = serviceFrom;
                item.Category = categoryFrom;
                item.Url = urlFrom;
                item.Login = loginFrom;
                item.Password = passwordFrom;
            }
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