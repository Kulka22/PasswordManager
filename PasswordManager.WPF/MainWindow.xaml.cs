using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
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


    private RelayCommand changeDataCommand;
    public RelayCommand ChangeDataCommand
    {
        get
        {
            return changeDataCommand ?? (changeDataCommand = new RelayCommand(() =>
            {

            }));
        }
    }

    private RelayCommand deleteDataCommand;
    public RelayCommand DeleteDataCommand
    {
        get
        {
            return deleteDataCommand ?? (deleteDataCommand = new RelayCommand(() => { }));
        }
    }

    public ItemViewModel(PasswordEntry item)
    {
        _passwordEntry = item;
        Url = item.Url;
        PasswordText = new string('*', _passwordEntry.Password.Length);
        LoginText = new string('*', _passwordEntry.Login.Length);

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
        public void AddData(object sender, RoutedEventArgs e)
        {
            Window changeDataWindow = new ChangeDataWindow();
            if (changeDataWindow.ShowDialog() == true)
            {

            }
            
        }
    }
}