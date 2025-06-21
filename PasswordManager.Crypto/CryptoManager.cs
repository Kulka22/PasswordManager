using System.Security.Cryptography;
using System.Text;
using PasswordManager.Core;

namespace PasswordManager.Crypto
{
    public class CryptoManager
    {
        public class EncryptManager
        {
            //public string EncryptPasswords(List<PasswordEntry> entries, byte[] dataKey)
            //{
            //    byte[] salt = KeyManager.GenerateSalt(); // Новая соль при каждом шифровании
            //    byte[] key = DeriveDataKey(dataKey, salt); // Доп. производный ключ

            //    string json = JsonConvert.SerializeObject(entries);
            //    byte[] encrypted = AesEncrypt(Encoding.UTF8.GetBytes(json), key);

            //    // Комбинируем соль + данные
            //    byte[] result = new byte[salt.Length + encrypted.Length];
            //    Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
            //    Buffer.BlockCopy(encrypted, 0, result, salt.Length, encrypted.Length);

            //    return Convert.ToBase64String(result);
            //}

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

            public static string EncryptJsonToMemory(string json, byte[] key)
            {
                byte[] iv = new byte[16];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(iv);

                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                using var memoryStream = new MemoryStream();
                memoryStream.Write(iv, 0, iv.Length);

                using var cryptoStream = new CryptoStream(memoryStream,
                    aes.CreateEncryptor(), CryptoStreamMode.Write);

                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                cryptoStream.Write(jsonBytes, 0, jsonBytes.Length);
                cryptoStream.FlushFinalBlock();

                return Convert.ToBase64String(memoryStream.ToArray());
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

            public static string DecryptJsonFromMemory(string encryptedBase64, byte[] key)
            {
                byte[] encryptedData = Convert.FromBase64String(encryptedBase64);

                using var memoryStream = new MemoryStream(encryptedData);
                byte[] iv = new byte[16];
                memoryStream.Read(iv, 0, iv.Length);

                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                using var cryptoStream = new CryptoStream(memoryStream,
                    aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var resultStream = new MemoryStream();

                cryptoStream.CopyTo(resultStream);
                return Encoding.UTF8.GetString(resultStream.ToArray());
            }
        }
    }
}
