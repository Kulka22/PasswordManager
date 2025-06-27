using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using PasswordManager.Core;
using PasswordManager.WPF;
using static PasswordManager.Data.DataManager.JsonManager;

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    public RelayCommand(Action execute) => _execute = execute;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged;
}


namespace PasswordManager.WPF
{
    public partial class MainWindow : Window
    {
        private MainProcess mainProcess;
        public class ItemViewModel : INotifyPropertyChanged
        {
            private PasswordEntry _passwordEntry;
            private string _url;
            private string _loginText;
            private string _passwordText;
            public string Url
            {
                get => _url;
                set => _url = value;
            }
            public string LoginText
            {
                get => _loginText;
                set
                {
                    _loginText = value;
                    OnPropertyChanged(nameof(LoginText));
                }
            }
            public string PasswordText
            {
                get => _passwordText;
                set
                {
                    _passwordText = value;
                    OnPropertyChanged(nameof(PasswordText));
                }
            }

            private RelayCommand changeVisibilityCommand;
            public RelayCommand ChangeVisibilityCommand
            {
                get
                {
                    return changeVisibilityCommand ?? (changeVisibilityCommand = new RelayCommand(() =>
                    {
                        if (LoginText.StartsWith("*"))
                        {
                            LoginText = _passwordEntry.Login;
                            PasswordText = _passwordEntry.Password;
                        }
                        else
                        {
                            LoginText = new string('*', _passwordEntry.Login.Length);
                            PasswordText = new string('*', _passwordEntry.Password.Length);
                        }
                    }));
                }
            }
            public RelayCommand ChangeDataCommand { get; }
            public RelayCommand DeleteDataCommand { get; }

            public ItemViewModel(PasswordEntry item, Action<PasswordEntry> changeDataCommand, Action<PasswordEntry> deleteDataCommand)
            {
                _passwordEntry = item;
                Url = item.Url;
                PasswordText = new string('*', _passwordEntry.Password.Length);
                LoginText = new string('*', _passwordEntry.Login.Length);

                ChangeDataCommand = new RelayCommand(() => changeDataCommand(item));
                DeleteDataCommand = new RelayCommand(() => deleteDataCommand(item));
            }
            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<ItemViewModel> Items { get; } = new ObservableCollection<ItemViewModel>();
        public MainWindow()
        {

            InitializeComponent();
            DataContext = this;
            if (MainProcess.CheckMasterPassword("qwerty", "psw.json"))
            {
                mainProcess = new MainProcess("qwerty");
                foreach (PasswordEntry item in mainProcess.GetPasswords())
                {
                    Items.Add(new ItemViewModel(item, value => ChangeData(value), value => DeleteData(value)));
                }
            }
        }
        public void AddData(object sender, RoutedEventArgs e)
        {
            ChangeDataWindow changeDataWindow = new ChangeDataWindow();
            if (changeDataWindow.ShowDialog() == true)
            {
                PasswordEntry newItem = new PasswordEntry();
                newItem.Url = changeDataWindow.URL;
                newItem.Login = changeDataWindow.NewLogin;
                newItem.Password = changeDataWindow.NewPassword;
                mainProcess.AddPassword(newItem);
                mainProcess.SavePasswords();
            }
            
        }

        private void ChangeData(PasswordEntry item)
        {
            ChangeDataWindow changeDataWindow = new ChangeDataWindow();
            changeDataWindow.URL = item.Url;
            changeDataWindow.NewLogin = item.Login;
            changeDataWindow.NewPassword = item.Password;

            if (changeDataWindow.ShowDialog() == true)
            {
                PasswordEntry newItem = item;
                newItem.Url = changeDataWindow.URL;
                newItem.Login = changeDataWindow.NewLogin;
                newItem.Password = changeDataWindow.NewPassword;
                mainProcess.ChangePassword(item.ID, newItem);
            }
            mainProcess.SavePasswords();
        }
        private void DeleteData(PasswordEntry item)
        {

        }


    }
}