using Newtonsoft.Json;
using PasswordManager.Crypto;
using PasswordManager.Data;
using static PasswordManager.Data.DataManager.JsonManager;
using PasswordManager.Core;
using System.Text;
using System.Security.Cryptography;

namespace PasswordManager.Tests
{
    public class UnitTests
    {
        // Модуль Crypto: CryptoManager
        // Протестирован функционал шифровки сообщения и его дешифровки
        [Fact]
        public void EncryptAndDecrypt_Message_ReturnsTheSameMessage()
        {
            string originalMessage = "Hello, world!";
            string password = "qwerty";
            byte[] salt = KeyManager.GenerateSalt();
            byte[] key = KeyManager.MakeKey(password, salt);
            byte[] messageAsByteArray = DataManager.EncodeManager.MakeByteArr(originalMessage);

            byte[] encryptedData = CryptoManager.EncryptManager.EncryptData(messageAsByteArray, key);
            byte[] decryptedData = CryptoManager.DecryptManager.DecryptData(encryptedData, key);
            string comparedStr = DataManager.EncodeManager.MakeStringFromByteArr(decryptedData);

            Assert.Equal(originalMessage, comparedStr);
        }

        [Fact]
        public void EncryptAndDecrypt_MessageAndNewSaltedKey_ReturnsException()
        {
            string originalMessage = "Hello, world!";
            string password = "qwerty";
            byte[] salt = KeyManager.GenerateSalt();
            byte[] key = KeyManager.MakeKey(password, salt);
            byte[] messageAsByteArray = DataManager.EncodeManager.MakeByteArr(originalMessage);
            byte[] newSalt = KeyManager.GenerateSalt();
            byte[] newKey = KeyManager.MakeKey(password, newSalt);

            byte[] encryptedData = CryptoManager.EncryptManager.EncryptData(messageAsByteArray, key);

            var exception = Assert.Throws<CryptographicException>(
                () => CryptoManager.DecryptManager.DecryptData(encryptedData, newKey));
        }

        [Fact]
        public void EncryptAndDecrypt_MessageAndIncorrectMasterPassword_ReturnsException()
        {
            string originalMessage = "Hello, world!";
            string password = "qwerty";
            byte[] salt = KeyManager.GenerateSalt();
            byte[] key = KeyManager.MakeKey(password, salt);
            byte[] messageAsByteArray = DataManager.EncodeManager.MakeByteArr(originalMessage);
            string incorrectPsw = "qvertu";
            byte[] newKey = KeyManager.MakeKey(incorrectPsw, salt);

            byte[] encryptedData = CryptoManager.EncryptManager.EncryptData(messageAsByteArray, key);

            var exception = Assert.Throws<CryptographicException>(
                () => CryptoManager.DecryptManager.DecryptData(encryptedData, newKey));
        }

        // Модуль Crypto: KeyManager
        // Протестирован функционал проверки соответствия хэшированного мастер-пароля
        [Fact]
        public void ComparePasswords_OrigPswAndHashedOrigPsw_ReturnsTrue()
        {
            string originalPsw = "_so-me_@Pass_word!";
            byte[] hashPsw = SHA256.HashData(Encoding.UTF8.GetBytes(originalPsw));

            bool result = KeyManager.ComparePasswords(originalPsw, hashPsw);

            Assert.True(result);
        }

        [Fact]
        public void ComparePasswords_OrigPswAndHashedIncorrectPsw_ReturnsFalse()
        {
            string originalPsw = "_so-me_@Pass_word!";
            string incorrectPsw = "_so-me_@Pass_vord!";
            byte[] hashPsw = SHA256.HashData(Encoding.UTF8.GetBytes(originalPsw));

            bool result = KeyManager.ComparePasswords(incorrectPsw, hashPsw);

            Assert.False(result);
        }


    }
}