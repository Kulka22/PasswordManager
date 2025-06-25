using PasswordManager.Data;
using PasswordManager.Crypto;
using static PasswordManager.Data.DataManager.JsonManager;

namespace PasswordManager.Core
{
    public class MainProcess
    {
        private string _masterPassword;
        private List<PasswordEntry> _passwords;
        private static bool _isStartAllowed = false;
        public readonly string _filePath;

        public MainProcess(string inputPassword, string filePath = "psw.json")
        {
            if (!_isStartAllowed && File.Exists(filePath))
                throw new Exception("YOU DONT HAVE ACCESS!!!!!!!!");
            _filePath = filePath;
            _masterPassword = inputPassword;
            _passwords = LoadData(_masterPassword, _filePath);
        }

        // Проверяем, есть ли файл и нужна ли регистрация
        public static bool GetRegStatus(string filePath)
        {
            return File.Exists(filePath);
        }

        // Метод для предупреждения о совпадающих паролях (Опциональный)
        // P.s. логины могут быть разные, речь ТОЛЬКО о паролях
        public bool CheckUniqueness(PasswordEntry inputPassword)
        {
            foreach (PasswordEntry password in _passwords)
                if (password.Password == inputPassword.Password)
                    return false;
            return true;
        }

        // А это уже метод для поиска совпадений у (login + site address)
        // Возвращает кортеж (nullable)
        // Если второй возвращаемый параметр true, то совпадение - ПОЛНОЕ
        public (PasswordEntry, bool)? FindRepetition(PasswordEntry inputPassword)
        {
            foreach (PasswordEntry password in _passwords)
            {
                if (password.Service == inputPassword.Service &&
                    password.Url == inputPassword.Url && password.Login == password.Login)
                {
                    if (password.Password == inputPassword.Password)
                        return (password, true);
                    return (password, false);
                }
            }
            return null;
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

            bool result = KeyManager.ComparePasswords(inputPassword, storedMasterHash, salt);
            if (result)
                _isStartAllowed = true;

            return result;
        }
    }
}
