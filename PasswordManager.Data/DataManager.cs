using Newtonsoft.Json;
using static PasswordManager.Crypto.CryptoManager;
using static PasswordManager.Crypto.KeyManager;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static PasswordManager.Core.Core;

namespace PasswordManager.Data
{
    public class DataManager
    {
        public class FileManager
        {
            public static byte[] ReadEncryptedFile(string filePath)
            {
                return File.Exists(filePath) ? File.ReadAllBytes(filePath) : null;
            }

            // Записывает уже зашифрованные данные в файл
            public static void WriteEncryptedFile(string filePath, byte[] encryptedData)
            {
                File.WriteAllBytes(filePath, encryptedData);
            }
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
                        File.Replace(tempFile, filePath, null);
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
                    throw new FileNotFoundException("Vault file not found.", filePath);

                byte[] fileBytes = File.ReadAllBytes(filePath);

                // Размеры соли и хэша (зависит от используемых алгоритмов)
                const int SaltSize = 16; // 128-битная соль
                const int HashSize = 32; // SHA256 = 256 бит = 32 байта

                if (fileBytes.Length < SaltSize + HashSize)
                    throw new InvalidDataException("File is corrupted or too small.");

                byte[] salt = new byte[SaltSize];
                byte[] storedMasterHash = new byte[HashSize];
                byte[] encryptedJson = new byte[fileBytes.Length - SaltSize - HashSize];

                Buffer.BlockCopy(fileBytes, 0, salt, 0, SaltSize);
                Buffer.BlockCopy(fileBytes, SaltSize, storedMasterHash, 0, HashSize);
                Buffer.BlockCopy(fileBytes, SaltSize + HashSize, encryptedJson, 0, encryptedJson.Length);

                // Проверка мастер-пароля
                byte[] inputMasterHash = SHA256.HashData(Encoding.UTF8.GetBytes(masterPassword));
                if (!CryptographicOperations.FixedTimeEquals(inputMasterHash, storedMasterHash))
                    throw new UnauthorizedAccessException("Incorrect master password.");

                byte[] key = MakeKey(masterPassword, salt);

                // Расшифровка
                byte[] decryptedBytes = DecryptManager.DecryptData(encryptedJson, key);
                string json = EncodeManager.MakeStringFromByteArr(decryptedBytes);

                var passwords = JsonConvert.DeserializeObject<List<PasswordEntry>>(json);
                return passwords ?? new List<PasswordEntry>();
            }


            public static void UpdatePassword(PasswordEntry newEntry, byte[] key)
            {
                // 0. Проверяем существование файла
                bool isFirstRun = !File.Exists(_filePath);
                List<PasswordEntry> passwords;

                // 1. Загружаем существующие данные или создаём новый список
                if (!isFirstRun)
                {
                    byte[] encryptedData = File.ReadAllBytes(_filePath);
                    byte[] jsonByteArr = DecryptManager.DecryptData(encryptedData, key);
                    string strJson = EncodeManager.MakeStringFromByteArr(jsonByteArr);
                    passwords = JsonConvert.DeserializeObject<List<PasswordEntry>>(strJson);
                    CryptographicOperations.ZeroMemory(encryptedData);
                }
                else
                {
                    passwords = new List<PasswordEntry>();
                }

                // 2. Добавляем новую запись
                passwords.Add(newEntry);
                string updatedJsonStr = JsonConvert.SerializeObject(passwords);
                byte[] updJsonByteArr = EncodeManager.MakeByteArr(updatedJsonStr);

                // 3. Шифруем данные
                byte[] newEncryptedData = EncryptManager.EncryptData(updJsonByteArr, key);

                // 4. Безопасное сохранение
                string tempFile = Path.GetTempFileName();
                try
                {
                    File.WriteAllBytes(tempFile, newEncryptedData);

                    // Разная логика для первого и последующих запусков
                    if (isFirstRun)
                    {
                        File.Move(tempFile, _filePath);
                    }
                    else
                    {
                        File.Replace(tempFile, _filePath, null);
                    }
                }
                finally
                {
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);
                }

                // 5. Очистка следов в памяти
                CryptographicOperations.ZeroMemory(newEncryptedData);
            }
        }
    }
}