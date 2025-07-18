﻿using PasswordManager.Data;
using PasswordManager.Crypto;
using static PasswordManager.Data.DataManager.JsonManager;
using System.Text;

namespace PasswordManager.Core
{
    public class MainProcess
    {
        private string _masterPassword;
        private List<PasswordEntry> _passwords;
        private static bool _isStartAllowed = false;
        private readonly DataManager.IFileManager _fileManager;
        private static readonly string _appDataPath;
        private static string _filePath;
        private readonly string _testFilePath;

        static MainProcess()
        {
            _appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PasswordManager");

            if (!Directory.Exists(_appDataPath))
                Directory.CreateDirectory(_appDataPath);

            _filePath = Path.Combine(_appDataPath, "psw.json");
        }

        public MainProcess(string inputPassword, DataManager.IFileManager fileManager = null,
            List<PasswordEntry> passwords = null, string filePath = null)
        {
            if (filePath != null)
                _filePath = filePath;
            if (fileManager == null)
                _fileManager = new DataManager.FileManager();
            else
                _fileManager = fileManager;
            if (!_isStartAllowed && _fileManager.Exists(_filePath))
                throw new Exception("YOU DONT HAVE ACCESS!");
            if (inputPassword == null || inputPassword.Length == 0)
                throw new Exception("MASTER-PASSWORD MUST BE SET!");

            _masterPassword = inputPassword;

            if (passwords == null)
                _passwords = LoadData(_masterPassword, _filePath, _fileManager);
            else
                _passwords = passwords;
            _isStartAllowed = true;
        }

        // Проверяем, есть ли файл и нужна ли регистрация
        public static bool GetRegStatus(string filePath = null)
        {
            if (filePath == null)
                return File.Exists(_filePath);
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
            List<PasswordEntry> passwords = GetPasswords();
            foreach (PasswordEntry password in passwords)
            {
                if (((password.Service == inputPassword.Service && password.Service.Length > 0) ||
                    (password.Url == inputPassword.Url && password.Url.Length > 0)) && 
                    password.Login == inputPassword.Login && inputPassword.Login.Length > 0)
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
            if (password.Password.Length == 0 || password.Password == null
                || ((password.Service == null || password.Service.Length == 0) &&
                (password.Url == null || password.Url.Length == 0)))
                throw new Exception("Required field are not filled in!");
            password.ID = Guid.NewGuid().ToString();
            if (password.Category != null && password.Category.Length > 0)
                password.Category = password.Category.ToLower();
            _passwords.Add(password);
        }

        public void RemovePassword(PasswordEntry passwordToDel)
        {
            for (int i = 0; i < _passwords.Count; i++)
            {
                if (_passwords[i].ID == passwordToDel.ID)
                {
                    _passwords.RemoveAt(i);
                    return;
                }
            }
            throw new Exception("ID not found");
        }

        public void ChangePassword(PasswordEntry changedPassword)
        {
            for (int i = 0; i < _passwords.Count; i++)
            {
                if (_passwords[i].ID == changedPassword.ID)
                {
                    if (changedPassword.Category != _passwords[i].Category)
                        changedPassword.Category = changedPassword.Category.ToLower();
                    _passwords[i] = changedPassword;
                    return;
                }
            }
            throw new Exception("ID not found");
        }

        public List<PasswordEntry> GetPasswords()
        {
            List<PasswordEntry> passwords = new List<PasswordEntry>();
            foreach (PasswordEntry password in _passwords)
            {
                passwords.Add(new PasswordEntry()
                {
                    ID = password.ID,
                    Service = password.Service,
                    Url = password.Url,
                    Login = password.Login,
                    Password = password.Password,
                    Category = password.Category
                });
            }
            return passwords;
        }

        public void SavePasswords()
        {
            SaveData(_passwords, _masterPassword, _filePath, _fileManager);
        }

        public static bool CheckMasterPassword(string inputPassword, string filePath = null)
        {
            if (filePath == null)
                filePath = _filePath;

            if (!File.Exists(filePath))
                throw new Exception("File not found!");

            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] storedMasterHash = new byte[HashSize];

            Buffer.BlockCopy(fileBytes, SaltSize, storedMasterHash, 0, HashSize);

            bool result = KeyManager.ComparePasswords(inputPassword, storedMasterHash);
            if (result)
                _isStartAllowed = true;

            return result;
        }

        public static string GeneratePassword(int length, List<char> forbiddenSymbols = null)
        {
            StringBuilder result = new StringBuilder();
            Random rnd = new Random();
            char rndChar;

            for (int i = 0; i < length; i++)
            {
                rndChar = (char)rnd.Next(33, 126);
                if (forbiddenSymbols != null && forbiddenSymbols.Contains(rndChar))
                {
                    --i;
                    continue;
                }
                result.Append(rndChar);
            }

            return result.ToString();
        }

        public Dictionary<string, List<PasswordEntry>> FilterCategories(List<string> categories)
        {
            Dictionary<string, List<PasswordEntry>> result =
                new Dictionary<string, List<PasswordEntry>>();
            List<PasswordEntry> passwords = GetPasswords();

            foreach (string category in categories)
            {
                foreach (PasswordEntry password in passwords)
                {
                    if (category == password.Category)
                    {
                        if (!result.ContainsKey(category))
                            result[category] = new List<PasswordEntry>();
                        result[category].Add(password);
                    }
                }    
            }

            return result;
        }

        public List<string> GetAllCategories()
        {
            List<string> result = new List<string>();
            string category;
            foreach (PasswordEntry password in _passwords)
            {
                if (password.Category != null && password.Category.Length > 0 &&
                    !result.Contains(password.Category))
                    result.Add(password.Category);
            }
            return result;
        }

        // Метод для "скрытного" ввода пароля, ТОЛЬКО для консоли!!
        public static string ReadPasswordConsole()
        {
            string password = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Backspace)
                {
                    password = password.Remove(password.Length - 1);
                    Console.Write("\b \b");
                }
                else if (key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
            }
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return password;
        }
    }
}