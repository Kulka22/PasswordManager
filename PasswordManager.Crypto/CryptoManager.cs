using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Crypto
{
    public class CryptoManager
    {
        public class EncryptManager
        {
            public static byte[] EncryptData(byte[] plainData, byte[] key)
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.GenerateIV();

                    using (var ms = new MemoryStream())
                    {
                        // Записываем IV в начало
                        ms.Write(aes.IV, 0, aes.IV.Length);

                        // Шифруем данные
                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(plainData, 0, plainData.Length);
                        }

                        return ms.ToArray();
                    }
                }
            }
        }

        public class DecryptManager
        {
            public static byte[] DecryptData(byte[] encryptedData, byte[] key)
            {
                using (Aes aes = Aes.Create())
                {
                    // Извлекаем IV (первые 16 байт)
                    byte[] iv = new byte[16];
                    Array.Copy(encryptedData, 0, iv, 0, 16);

                    aes.Key = key;
                    aes.IV = iv;

                    // Расшифровываем остальные данные
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(
                            new MemoryStream(encryptedData, 16, encryptedData.Length - 16),
                            aes.CreateDecryptor(),
                            CryptoStreamMode.Read))
                        {
                            cs.CopyTo(ms);
                        }

                        return ms.ToArray();
                    }
                }
            }
        }
    }
}
