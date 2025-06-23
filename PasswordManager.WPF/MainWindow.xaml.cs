using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PasswordManager.Core;
using static PasswordManager.Data.DataManager.JsonManager;

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    public RelayCommand(Action execute) => _execute = execute;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged;
}
public class ItemViewModel : INotifyPropertyChanged
{
    private string _id;
    private string _addres;
    private string _login;
    private string _loginText;
    private string _password;
    private string _passwordText;
    private string _category;
    public string Id
    {
        get => _id;
        set => _id = value;
    }
    public string Addres
    {
        get => _addres;
        set => _addres = value;
    }
    public string Login
    {
        get => _login;
        set { _login = value; OnPropertyChanged(nameof(Login)); }
    }
    public string LoginText
    {
        get => _loginText;
        set => _loginText = new string('*', value.Length);
    }
    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(nameof(Password)); }
    }
    public string PasswordText
    {
        get => _passwordText;
        set => _passwordText = new string('*', value.Length);
    }
    public string Category
    {
        get => _category;
        set => _category = value;
    }
    public ICommand ChangeCommand { get; }
    public ICommand DeleteCommand { get; }

    public void ChangeVisibility ()
    {
        if (LoginText.StartsWith("*"))
        {
            LoginText = Login;
            PasswordText = Password;
        }
    }
    public ItemViewModel(PasswordEntry item)
    {
        Id = item.ID;
        Addres = item.Url;
        Login = item.Login;
        LoginText = "";
        Password = item.Password;
        PasswordText = "";
        Category = item.Category;
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
namespace PasswordManager.WPF
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<ItemViewModel> Items { get; } = new ObservableCollection<ItemViewModel>();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            MainProcess mainProcess = new MainProcess("qwerty");
            foreach (PasswordEntry item in mainProcess.GetPasswords())
            {
                Items.Add(new ItemViewModel(item));
            }
        }
        private void Add(object sender, RoutedEventArgs e) { }
        private void ChangeItem() { }
        private void DeleteItem(string str){ }
    }
}