using Newtonsoft.Json;
using PasswordManager.Crypto;
using PasswordManager.Data;
using static PasswordManager.Data.DataManager.JsonManager;

namespace PasswordManager.Tests
{
    public class UnitTest1
    {
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

        [Fact]
        public void Test2()
        {
            PasswordEntry pe = new PasswordEntry() { Service = "VK", Login = "abc",
            Password = "12345", Url = "vk.com"};
            string password = "qwerty";
            byte[] salt = KeyManager.GenerateSalt();
            byte[] key = KeyManager.MakeKey(password, salt);

            UpdatePassword(pe, key);

            byte[] encrJsonArr = DataManager.FileManager.ReadEncryptedFile("psw.json");
            byte[] decrJsonArr = CryptoManager.DecryptManager.DecryptData(encrJsonArr, key);
            string jsonStr = DataManager.EncodeManager.MakeStringFromByteArr(decrJsonArr);
            var passwords = JsonConvert.DeserializeObject<List<PasswordEntry>>(jsonStr);
            string result = $"{passwords[0].Service} {passwords[0].Url} " +
                $"{passwords[0].Login} {passwords[0].Password}";

            Assert.Equal("VK vk.com abc 12345", result);
        }
    }
}