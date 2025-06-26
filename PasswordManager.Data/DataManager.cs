using Newtonsoft.Json;
using static PasswordManager.Crypto.CryptoManager;
using static PasswordManager.Crypto.KeyManager;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PasswordManager.Data
{
    public class DataManager
    {
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
            }

            public static void SaveData(List<PasswordEntry> passwords,
                string masterPassword, string filePath)
            {
                // Каждый раз при сохранении генерируем новую соль
                byte[] salt = GenerateSalt();
                byte[] masterHash = SHA256.HashData(Encoding.UTF8.GetBytes(masterPassword));
                byte[] newKey = MakeKey(masterPassword, salt);

                bool isFirstRun = !File.Exists(filePath);

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
                string tempFile = Path.GetTempFileName();
                try
                {
                    File.WriteAllBytes(tempFile, result);
                    if (isFirstRun)
                        File.Move(tempFile, filePath);
                    else
                        File.Copy(tempFile, filePath, overwrite: true);
                }
                finally
                {
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);
                }

                // Очистка следов в памяти
                CryptographicOperations.ZeroMemory(encryptedJson);
                CryptographicOperations.ZeroMemory(result);
            }

            public static List<PasswordEntry> LoadData(string masterPassword, string filePath)
            {
                if (!File.Exists(filePath))
                {
                    return new List<PasswordEntry>();
                }

                byte[] fileBytes = File.ReadAllBytes(filePath);

                if (fileBytes.Length < SaltSize + HashSize)
                    throw new InvalidDataException("File is corrupted!");

                byte[] salt = new byte[SaltSize];
                byte[] storedMasterHash = new byte[HashSize];
                byte[] encryptedJson = new byte[fileBytes.Length - SaltSize - HashSize];

                Buffer.BlockCopy(fileBytes, 0, salt, 0, SaltSize);
                Buffer.BlockCopy(fileBytes, SaltSize, storedMasterHash, 0, HashSize);
                Buffer.BlockCopy(fileBytes, SaltSize + HashSize, encryptedJson, 0, encryptedJson.Length);

                // Проверка мастер-пароля
                byte[] inputMasterHash = SHA256.HashData(Encoding.UTF8.GetBytes(masterPassword));
                if (!CryptographicOperations.FixedTimeEquals(inputMasterHash, storedMasterHash))
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