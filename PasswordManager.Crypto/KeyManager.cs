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
        //private const int AesBlockSize = 128;
        // Количество итераций для PBKDF2
        private const int Iterations = 100000;

        public static byte[] MakeKey(string password, byte[] salt)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            return deriveBytes.GetBytes(32);
        }

        public static bool ComparePasswords(string inputPassword, byte[] storedHash)
        {
            byte[] inputHash = SHA256.HashData(Encoding.UTF8.GetBytes(inputPassword));
            return CryptographicOperations.FixedTimeEquals(inputHash, storedHash);
        }

        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }
    }
}
