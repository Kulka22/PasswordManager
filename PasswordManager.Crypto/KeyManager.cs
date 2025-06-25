using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Crypto
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
            //byte[] saltedPassword = new byte[Encoding.UTF8.GetByteCount(inputPassword) + salt.Length];
            //Encoding.UTF8.GetBytes(inputPassword).CopyTo(saltedPassword, 0);
            //salt.CopyTo(saltedPassword, Encoding.UTF8.GetByteCount(inputPassword));
            //byte[] inputHash = SHA256.HashData(saltedPassword);

            byte[] inputHash = SHA256.HashData(Encoding.UTF8.GetBytes(inputPassword));
            return CryptographicOperations.FixedTimeEquals(inputHash, storedHash);
        }

        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16]; // 128-битная соль
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }
    }
}
