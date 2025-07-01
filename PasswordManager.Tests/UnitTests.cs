using Newtonsoft.Json;
using PasswordManager.Crypto;
using PasswordManager.Data;
using static PasswordManager.Data.DataManager.JsonManager;
using PasswordManager.Core;

namespace PasswordManager.Tests
{
    public class UnitTests
    {
        // Модуль Crypto: CryptoManager
        // Протестирован функционал шифровки сообщения и его дешифровки
        [Fact]
        public void EncryptAndDecrypt_Message_ReturnTheSameMessage()
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


    }
}