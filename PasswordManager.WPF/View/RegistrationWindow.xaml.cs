using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PasswordManager.WPF
{
    public partial class RegistrationWindow : Window
    {
        private string _lastPass = "";
        private string _newPass;

        private const string prohibitedSymbols = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя @/?";
        private bool lowCharacters = false;
        private bool highCharacters = false;
        private const string consecutiveCharactersString = "aaa bbb ccc ddd eee fff ggg hhh iii jjj kkk lll mmm nnn ooo ppp qqq rrr sss ttt uuu vvv www xxx yyy zzz qwertyuiop asdfghjkl zxcvbnm,. qaz wsx edc rfv tgb yhn ujm ik, ol. 01234567890 09876543210 poiuytrewq lkjhgfdsa mnbvcxz zaq xsw cde vfr bgt nhy mju abcdefghijklmnopqrstuvwxyz 111 222 333 444 555 666 777 888 999 000";

        private bool useNumbers = false;
        private const string letters = "abcdefghijklmnopqrstuvwxyz";
        private bool useLetters = false;
        private bool useSymbols = false;





        private string NewPass { get; set; }
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void SetStart()
        {
            lowCharacters = false;
            highCharacters = false;
            useNumbers = false;
            useLetters = false;
            useSymbols = false;

            passLenLabel.Foreground = Brushes.Red;
            consecutiveCharactersLabel.Foreground = Brushes.Green;
            prohibitedSymbolsLabel.Foreground = Brushes.Green;
            characterRegisterLabel.Foreground = Brushes.Red;
            allTypeSymbolsLabel.Foreground = Brushes.Red;
        }
        private void CheckPassword(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;

            _newPass = textBox.Text;
            if (_newPass.Length < _lastPass.Length)
            {
                _lastPass = "";
                SetStart();
                CheckPassword(sender, e);
            }
            else
            {
                for (int i = _lastPass.Length; i < _newPass.Length; i++)
                {
                    if (prohibitedSymbols.Contains(char.ToLower(_newPass[i])))
                    {
                        continue;
                    }

                    _lastPass += _newPass[i];

                    

                    if (letters.Contains(char.ToLower(_newPass[i])))
                    {
                        useLetters = true;

                        if (char.ToUpper(_newPass[i]) == _newPass[i]) highCharacters = true;
                        else lowCharacters = true;

                        if (lowCharacters && highCharacters)
                        {
                            characterRegisterLabel.Foreground = Brushes.Green;
                        }
                        if (useLetters && useNumbers && useSymbols)
                        {
                            allTypeSymbolsLabel.Foreground = Brushes.Green;
                        }
                    }
                    else if (char.IsDigit(_newPass[i]))
                    {
                        useNumbers = true;
                        if (useLetters && useNumbers && useSymbols)
                        {
                            allTypeSymbolsLabel.Foreground = Brushes.Green;
                        }
                    }
                    else
                    {
                        useSymbols = true;
                        if (useLetters && useNumbers && useSymbols)
                        {
                            allTypeSymbolsLabel.Foreground = Brushes.Green;
                        }
                    }

                    if (_lastPass.Length > 2)
                    {
                        if (consecutiveCharactersString.Contains(_lastPass.ToLower().Substring(_lastPass.Length - 3)))
                        {
                            consecutiveCharactersLabel.Foreground = Brushes.Red;
                        }
                    }

                    if (_lastPass.Length > 11 & _lastPass.Length < 21) passLenLabel.Foreground = Brushes.Green;
                    else passLenLabel.Foreground = Brushes.Red;


                }
            }

            textBox.Text = _lastPass;
        }

        private void CheckMatch(object sender, TextChangedEventArgs e)
        {
            if (EntryPassword.Text == CheckEntryPassword.Text)
            {
            }
            else
            {
            }
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            if ((EntryPassword.Text == CheckEntryPassword.Text) && ((lowCharacters && highCharacters) && useLetters && useNumbers && useSymbols))
            {
                DialogResult = true;
                Close();
            }
            else
            {
            }
        }
    }
}
