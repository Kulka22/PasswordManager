using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Crypto
{
    public class Crypto
    {
        public class KeyManager
        {
            // Размер блока AES (128 бит)
            private const int AesBlockSize = 128;
            // Количество итераций для PBKDF2
            private const int Iterations = 100000;

            public static byte[] MakeKey(string password, byte[] salt)
            {
                // Используем PBKDF2 для более безопасного ключа
                using var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
                return deriveBytes.GetBytes(32); // 256-битный ключ для AES
            }

            public static bool ComparePasswords(string inputPassword, byte[] storedHash, byte[] salt)
            {
                byte[] saltedPassword = new byte[Encoding.UTF8.GetByteCount(inputPassword) + salt.Length];
                Encoding.UTF8.GetBytes(inputPassword).CopyTo(saltedPassword, 0);
                salt.CopyTo(saltedPassword, Encoding.UTF8.GetByteCount(inputPassword));

                byte[] inputHash = SHA256.HashData(saltedPassword);
                return CryptographicOperations.FixedTimeEquals(inputHash, storedHash);
            }

            public static byte[] GenerateSalt()
            {
                byte[] salt = new byte[16]; // 128-битная соль
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(salt);
                return salt;
            }

            //public static byte[] MakeKey(string password)
            //{
            //    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            //    byte[] hashPassword = SHA256.HashData(passwordBytes);

            //    return hashPassword; //Возвращает хэшированый мастер-пароль
            //}

            //public static bool ComparePasswords(string inputPassword, 
            //    byte[] realHashPassword)
            //{
            //    byte[] passwordBytes = Encoding.UTF8.GetBytes(inputPassword);

            //    byte[] hashPassword = SHA256.HashData(passwordBytes);

            //    //Сравниваем хэшированные версии паролей
            //    if (realHashPassword.SequenceEqual(hashPassword))
            //        return true;
            //    return false;
            //}
        }

        public class CryptoManager
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

            // Опциональный класс, мб удалю
            public class CryptoJsonManager
            {
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
}
