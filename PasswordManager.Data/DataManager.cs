using Newtonsoft.Json;
using static PasswordManager.Crypto.CryptoManager;
using static PasswordManager.Crypto.KeyManager;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PasswordManager.Crypto;

namespace PasswordManager.Data
{
    public class DataManager
    {
        public interface IFileManager
        {
            bool Exists(string path);
            byte[] Read(string path);
            void Write(string path, byte[] data);
            void Move(string sourcePath, string destPath);
            void Copy(string sourcePath, string destPath, bool overwrite);
            void Delete(string path);
            string GetTempFileName();
        }

        public class FileManager : IFileManager
        {
            public void Copy(string sourcePath, string destPath, bool overwrite)
                => File.Copy(sourcePath, destPath, overwrite);

            public void Delete(string path) => File.Delete(path);

            public bool Exists(string path) => File.Exists(path);

            public void Move(string sourcePath, string destPath) 
                => File.Move(sourcePath, destPath);

            public byte[] Read(string path) => File.ReadAllBytes(path);

            public void Write(string path, byte[] data) => File.WriteAllBytes(path, data);
            public string GetTempFileName() => Path.GetTempFileName();
        }

        public class FileManagerTests : IFileManager
        {
            private readonly Dictionary<string, byte[]> _files = new();
            private int _tempFileCounter = 0;

            public void Copy(string sourcePath, string destPath, bool overwrite)
            {
                if (!_files.ContainsKey(sourcePath))
                    throw new FileNotFoundException($"Source file not found: {sourcePath}");
                if (_files.ContainsKey(destPath) && !overwrite)
                    throw new IOException($"Destination file already exists: {destPath}");
                _files[destPath] = (byte[])_files[sourcePath].Clone();
            }

            public void Delete(string path) => _files.Remove(path);

            public bool Exists(string path) => _files.ContainsKey(path);

            public string GetTempFileName()
            {
                string tempFileName = $"temp_{_tempFileCounter++}.tmp";
                _files[tempFileName] = Array.Empty<byte>();
                return tempFileName;
            }

            public void Move(string sourcePath, string destPath)
            {
                if (!_files.ContainsKey(sourcePath))
                    throw new FileNotFoundException($"Source file not found: {sourcePath}");
                if (_files.ContainsKey(destPath))
                    throw new IOException($"Destination file already exists: {destPath}");
                _files[destPath] = _files[sourcePath];
                _files.Remove(sourcePath);
            }

            public byte[] Read(string path)
            {
                if (!_files.ContainsKey(path))
                    throw new FileNotFoundException($"File not found: {path}");
                return (byte[])_files[path].Clone();
            }

            public void Write(string path, byte[] data) => _files[path] = data;
        }


        public class EncodeManager
        {
            public static byte[] MakeByteArr(string inputString)
            {
                return Encoding.UTF8.GetBytes(inputString);
            }

            public static string MakeStringFromByteArr(byte[] strAsByteArr)
            {
                return Encoding.UTF8.GetString(strAsByteArr);
            }
        }

        public class JsonManager
        {
            private const string _filePath = "psw.json";
            // Размеры соли и хэша (зависит от используемых алгоритмов)
            public const int SaltSize = 16; // 128-битная соль
            public const int HashSize = 32; // SHA256 = 256 бит = 32 байта

            public class PasswordEntry
            {
                public string ID { get; set; } = Guid.NewGuid().ToString();
                public string Service { get; set; }
                public string Url { get; set; }
                public string Category { get; set; }
                public string Login { get; set; }
                public string Password { get; set; }

                public override bool Equals(object? obj)
                {
                    if (obj is not PasswordEntry other)
                        return false;

                    return ID == other.ID &&
                           Service == other.Service &&
                           Url == other.Url &&
                           Category == other.Category &&
                           Login == other.Login &&
                           Password == other.Password;
                }
            }

            public static void SaveData(List<PasswordEntry> passwords,
                string masterPassword, string filePath, IFileManager fileManager)
            {
                // Каждый раз при сохранении генерируем новую соль
                byte[] salt = GenerateSalt();
                byte[] masterHash = SHA256.HashData(Encoding.UTF8.GetBytes(masterPassword));
                byte[] newKey = MakeKey(masterPassword, salt);

                bool isFirstRun = !fileManager.Exists(filePath);

                string jsonStr = JsonConvert.SerializeObject(passwords);
                byte[] jsonAsByteArr = EncodeManager.MakeByteArr(jsonStr);

                // Шифруем
                byte[] encryptedJson = EncryptManager.EncryptData(jsonAsByteArr, newKey);

                // Добавляем соль и хэшированный мастер-пароль в начало файла
                byte[] result = new byte[salt.Length + masterHash.Length + encryptedJson.Length];
                Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
                Buffer.BlockCopy(masterHash, 0, result, salt.Length, masterHash.Length);
                Buffer.BlockCopy(encryptedJson, 0, result, 
                    salt.Length + masterHash.Length, encryptedJson.Length);

                // Безопасно сохраняем
                string tempFile = fileManager.GetTempFileName();
                try
                {
                    fileManager.Write(tempFile, result);
                    if (isFirstRun)
                        fileManager.Move(tempFile, filePath);
                    else
                        fileManager.Copy(tempFile, filePath, true);
                }
                finally
                {
                    if (fileManager.Exists(tempFile))
                        fileManager.Delete(tempFile);
                }

                // Очистка следов в памяти
                CryptographicOperations.ZeroMemory(encryptedJson);
                CryptographicOperations.ZeroMemory(result);
            }

            public static List<PasswordEntry> LoadData(string masterPassword, 
                string filePath, IFileManager fileManager)
            {
                if (!fileManager.Exists(filePath))
                    return new List<PasswordEntry>();

                byte[] fileBytes = fileManager.Read(filePath);

                if (fileBytes.Length < SaltSize + HashSize)
                    throw new InvalidDataException("File is corrupted!");

                byte[] salt = new byte[SaltSize];
                byte[] storedMasterHash = new byte[HashSize];
                byte[] encryptedJson = new byte[fileBytes.Length - SaltSize - HashSize];

                // Копирование соли, хэша мастер-пароля и остальных данных из файла
                Buffer.BlockCopy(fileBytes, 0, salt, 0, SaltSize);
                Buffer.BlockCopy(fileBytes, SaltSize, storedMasterHash, 0, HashSize);
                Buffer.BlockCopy(fileBytes, SaltSize + HashSize, encryptedJson, 0, encryptedJson.Length);

                // Проверка мастер-пароля
                if (!ComparePasswords(masterPassword, storedMasterHash))
                    throw new UnauthorizedAccessException("Incorrect master password!");

                byte[] key = MakeKey(masterPassword, salt);

                // Расшифровка
                byte[] decryptedBytes = DecryptManager.DecryptData(encryptedJson, key);
                string json = EncodeManager.MakeStringFromByteArr(decryptedBytes);

                var passwords = JsonConvert.DeserializeObject<List<PasswordEntry>>(json);
                return passwords ?? new List<PasswordEntry>();
            }
        }
    }
}