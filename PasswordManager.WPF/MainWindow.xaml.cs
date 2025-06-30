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
            public PasswordEntry _passwordEntry;
            private string _servise;
            private string _url;
            private string _loginText;
            private string _passwordText;
            public string Service
            {
                get => _servise;
                set => _servise = value;
            }
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

            public RelayCommand ChangeVisibilityCommand { get; }
            public RelayCommand ChangeDataCommand { get; }
            public RelayCommand DeleteDataCommand { get; }

            public ItemViewModel(PasswordEntry item, Action<PasswordEntry> changeDataCommand, Action<ItemViewModel> deleteDataCommand)
            {
                _passwordEntry = item;
                Service = item.Service;
                Url = item.Url;
                PasswordText = new string('*', _passwordEntry.Password.Length);
                LoginText = new string('*', _passwordEntry.Login.Length);

                ChangeDataCommand = new RelayCommand(() =>
                {
                    changeDataCommand(item);
                    LoginText = item.Login;
                    PasswordText = item.Password;
                });

                DeleteDataCommand = new RelayCommand(() => deleteDataCommand(this));
                
                ChangeVisibilityCommand = new RelayCommand(() =>
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
                });
            }
            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<ItemViewModel> Items { get; } = new ObservableCollection<ItemViewModel>();
        public MainWindow()
        {
            if (!MainProcess.GetRegStatus("psw.json"))
            {
                RegistrationWindow registrationWindow = new RegistrationWindow();
                if (registrationWindow.ShowDialog() == true)
                {
                    Start(registrationWindow.EntryPassword.Text);
                    UpdateItems();
                }
                else
                {
                    Environment.Exit(1);
                }
            }
            else
            {
                SignInWindow signInWindow = new SignInWindow();
                bool? result = signInWindow.ShowDialog();
                if (result == true)
                {
                    Start(signInWindow.EntryPassword.Password);
                }
                else Environment.Exit(1);
            }
            DataContext = this;
        }

        private void Start(string master)
        {
            mainProcess = new MainProcess(master);
            UpdateItems();
        }
        public void AddData(object sender, RoutedEventArgs e)
        {
            AddDataWindow addDataWindow = new AddDataWindow();
            if (addDataWindow.ShowDialog() == true)
            {
                PasswordEntry newItem = new PasswordEntry();
                newItem.Service = addDataWindow.ServiceText.Text;
                newItem.Url = addDataWindow.UrlText.Text;
                newItem.Login = addDataWindow.LoginText.Text;
                newItem.Password = addDataWindow.PasswordText.Text;

                var tuple = mainProcess.FindRepetition(newItem);
                if (tuple != null)
                {
                    if (tuple.Value.Item2)
                    {
                        MessageWindow messageWindow = new MessageWindow();
                        messageWindow.MessageLabel.Content = "Такая запись уже существует!";
                        messageWindow.Show();
                    }
                    else
                    {
                        ConfirmationWindow confirmationWindow = new ConfirmationWindow();
                        confirmationWindow.Width = 350;
                        confirmationWindow.TextLabel.Content = "Существует запись с таким же Service и Login.\nИзменить пароль в уже существующей записи?";
                        confirmationWindow.TextLabel.Width = 350;
                        if (confirmationWindow.ShowDialog() == true)
                        {
                            tuple.Value.Item1.Password = newItem.Password;
                            mainProcess.ChangePassword(tuple.Value.Item1);
                            
                        }
                        else
                        {
                            mainProcess.AddPassword(newItem);
                        }
                        mainProcess.SavePasswords();
                        UpdateItems();
                    }
                }
                else
                {
                    mainProcess.AddPassword(newItem);
                    mainProcess.SavePasswords();
                    UpdateItems();
                }
                
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
                item.Url = changeDataWindow.URL;
                item.Login = changeDataWindow.NewLogin;
                item.Password = changeDataWindow.NewPassword;
                mainProcess.ChangePassword(item);
                mainProcess.SavePasswords();

                UpdateItems();
            }
        }
        private void DeleteData(ItemViewModel item)
        {
            ConfirmationWindow confirmationWindow = new ConfirmationWindow();
            if (confirmationWindow.ShowDialog() == true)
            {
                mainProcess.RemovePassword(item._passwordEntry);
                
                mainProcess.SavePasswords();
                UpdateItems();
            }
        }

        private void UpdateItems()
        {
            Items.Clear();
            List<PasswordEntry> passItems = mainProcess.GetPasswords();

            foreach (PasswordEntry item in passItems)
            {
                Items.Add(new ItemViewModel(item, value => ChangeData(value), value => DeleteData(value)));
            }
        }
    }
}