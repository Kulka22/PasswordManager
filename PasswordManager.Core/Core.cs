using PasswordManager.Data;
using PasswordManager.Crypto;
using static PasswordManager.Data.DataManager.JsonManager;

namespace PasswordManager.Core
{
    public class MainProcess
    {
        private string _masterPassword;
        private readonly string _filePath = "psw.json";
        private List<PasswordEntry> _passwords;

        public MainProcess(string masterPassword)
        {
            _masterPassword = masterPassword;
            _passwords = LoadData(masterPassword, _filePath);
        }

        public void AddPassword(PasswordEntry password)
        {
            password.ID = Guid.NewGuid().ToString();
            _passwords.Add(password);
        }

        public void ChangePassword(string ID, PasswordEntry changedPassword)
        {
            for (int i = 0; i < _passwords.Count; i++)
            {
                if (_passwords[i].ID == ID)
                {
                    _passwords[i] = changedPassword;
                    return;
                }
            }

            throw new Exception("ID not found");
        }

        public List<PasswordEntry> GetPasswords()
        {
            return _passwords;
        }

        public void SavePasswords()
        {
            SaveData(_passwords, _masterPassword, _filePath);
        }

        //public class PasswordEntry
        //{
        //    public string Service { get; set; }
        //    public string Url { get; set; }
        //    public string Category { get; set; }
        //    public string Login { get; set; }
        //    public string Password { get; set; }
        //}
    }
}
