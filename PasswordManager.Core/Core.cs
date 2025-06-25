using PasswordManager.Data;
using PasswordManager.Crypto;
using static PasswordManager.Data.DataManager.JsonManager;

namespace PasswordManager.Core
{
    public class MainProcess
    {
        private string _masterPassword;
        private readonly string _filePath;
        private List<PasswordEntry> _passwords;
        private bool _isStartAllowed = false;

        public MainProcess(string inputPassword, string filePath)
        {
            _filePath = filePath;
            _masterPassword = inputPassword;
            _passwords = LoadData(_masterPassword, _filePath);
        }

        // Проверяем, есть ли файл и нужна ли регистрация
        public bool GetRegStatus()
        {
            return File.Exists(_filePath);
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
        
        public static bool CheckMasterPassword(string inputPassword, string filePath)
        {
            if (!File.Exists(filePath))
                throw new Exception("Это первый запуск, проверять пароль не надо!");

            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] salt = new byte[SaltSize];
            byte[] storedMasterHash = new byte[HashSize];

            Buffer.BlockCopy(fileBytes, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(fileBytes, salt.Length, storedMasterHash, 0, HashSize);
            
            return KeyManager.ComparePasswords(inputPassword, storedMasterHash, salt);
        }
    }
}
