using Newtonsoft.Json;
using PasswordManager.Crypto;
using System.Text;

namespace PasswordManager.Data
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
}