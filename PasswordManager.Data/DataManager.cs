using Newtonsoft.Json;
using PasswordManager.Crypto;
using System.Security.Cryptography;
using System.Text;

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

            public class PasswordEntry
            {
                public string Service { get; set; }
                public string Url { get; set; }
                public string Login { get; set; }
                public string Password { get; set; }
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
                    byte[] jsonByteArr = CryptoManager.DecryptManager.DecryptData(encryptedData, key);
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
                byte[] newEncryptedData = CryptoManager.EncryptManager.EncryptData(updJsonByteArr, key);

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