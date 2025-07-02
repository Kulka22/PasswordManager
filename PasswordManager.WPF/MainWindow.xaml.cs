using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Windows;
using System.Windows.Controls;
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
        private List<string> categories = new List<string>();
        public class ItemViewModel : INotifyPropertyChanged
        {
            public PasswordEntry _passwordEntry;
            private string _servise;
            private string _category;
            private string _url;
            private string _loginText;
            private string _passwordText;
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
                Category = item.Category;
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
                    mainProcess = new MainProcess(registrationWindow.EntryPassword.Text);
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
                    mainProcess = new MainProcess(signInWindow.EntryPassword.Password);
                }
                else Environment.Exit(1);
            }
            DataContext = this;

        }

        private void Checked(object sender, RoutedEventArgs e)
        {
            var temp = (CheckBox)sender;
            categories.Add(temp.Content.ToString());
            UpdateItems();
        }
        private void Unchecked(object sender, RoutedEventArgs e)
        {
            var temp = (CheckBox)sender;
            categories.Remove(temp.Content.ToString());
            UpdateItems();
        }
        private void SetExpander()
        {
            StackPanel stackPanel = new StackPanel();
            foreach (var i in mainProcess.GetAllCategories())
            {
                var temp = new CheckBox();
                temp.Content = i;
                if (categories.Contains(i)) temp.IsChecked = true;
                temp.Checked += Checked;
                temp.Unchecked += Unchecked;
                stackPanel.Children.Add(temp);
            }
            CategoriesExpander.Content = stackPanel;
        }

        private void LoadedData(object sender, RoutedEventArgs e)
        {
            
            UpdateItems();
        }
        public void AddData(object sender, RoutedEventArgs e)
        {
            AddDataWindow addDataWindow = new AddDataWindow();
            if (addDataWindow.ShowDialog() == true)
            {
                PasswordEntry newItem = new PasswordEntry();
                newItem.Service = addDataWindow.ServiceText.Text;
                newItem.Category = addDataWindow.CategoryText.Text;
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
                            mainProcess.SavePasswords();
                            UpdateItems();

                        }
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
            AddDataWindow changeDataWindow = new AddDataWindow();
            changeDataWindow.Title = "Изменить запись";
            changeDataWindow.ServiceText.Text = item.Service;
            changeDataWindow.UrlText.Text = item.Url;
            changeDataWindow.CategoryText.Text = item.Category;
            changeDataWindow.LoginText.Text = item.Login;
            changeDataWindow.PasswordText.Text = item.Password;

            if (changeDataWindow.ShowDialog() == true)
            {
                item.Service = changeDataWindow.ServiceText.Text;
                item.Url = changeDataWindow.UrlText.Text;
                item.Category = changeDataWindow.CategoryText.Text;
                item.Login = changeDataWindow.LoginText.Text;
                item.Password = changeDataWindow.PasswordText.Text;
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
            SetExpander();
            Items.Clear();

            if (categories != null && categories.Count > 0)
            {
                Dictionary<string, List<PasswordEntry>> sortedPasswords = mainProcess.FilterCategories(categories);
                foreach (string category in sortedPasswords.Keys)
                {
                    foreach (PasswordEntry entry in sortedPasswords[category])
                    {
                        Items.Add(new ItemViewModel(entry, value => ChangeData(value), value => DeleteData(value)));
                    }
                }
                
            }
            else
            {
                List<PasswordEntry> passItems = mainProcess.GetPasswords();

                foreach (PasswordEntry item in passItems)
                {
                    Items.Add(new ItemViewModel(item, value => ChangeData(value), value => DeleteData(value)));
                }
            }
        }
    }
}