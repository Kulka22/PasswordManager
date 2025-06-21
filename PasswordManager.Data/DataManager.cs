using Newtonsoft.Json;
using PasswordManager.Crypto;
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
            public class PasswordEntry
            {
                public string Service { get; set; }
                public string Url { get; set; }
                public string Login { get; set; }
                public string Password { get; set; }
            }
        }
    }
}