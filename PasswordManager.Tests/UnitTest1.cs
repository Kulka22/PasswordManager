using Newtonsoft.Json;
using PasswordManager.Crypto;
using PasswordManager.Data;
using static PasswordManager.Data.DataManager.JsonManager;
using PasswordManager.Core;

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
        public void Test4()
        {
            MainProcess main = new MainProcess("qwerty");
            PasswordEntry password1 = new PasswordEntry()
            {
                Service = "VK",
                Category = "social",
                Login = "abc",
                Password = "12345",
                Url = "vk.ru"
            };
            PasswordEntry password2 = new PasswordEntry()
            {
                Service = "YouTube",
                Category = "video",
                Login = "lol123",
                Password = "0987",
                Url = "youtube.com"
            };
            main.AddPassword(password1);
            main.AddPassword(password2);
            main.SavePasswords();

            main = new MainProcess("qwerty");
            List<PasswordEntry> gettedPsws = main.GetPasswords();
            PasswordEntry psw1 = gettedPsws[0];
            PasswordEntry psw2 = gettedPsws[1];
            string result = $"{psw1.Service} {psw1.Password} {psw2.Service} {psw2.Password}";
            string expected = "VK 12345 YouTube 0987";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Test5()
        {
            MainProcess main = new MainProcess("qwerty");
            List<PasswordEntry> passwords = main.GetPasswords();
            PasswordEntry original = passwords[0];
            PasswordEntry password = new PasswordEntry()
            {
                ID = original.ID,
                Service = original.Service,
                Url = original.Url,
                Login = "NEWLOGIN",
                Password = original.Password,
                Category = original.Category
            };
            main.ChangePassword(password.ID, password);
            main.SavePasswords();

            main = new MainProcess("qwerty");
            List<PasswordEntry> gettedPsws = main.GetPasswords();
            //string expected = "2 VK newLogin 12345";
            string expected = $"{passwords.Count} {original.Service} NEWLOGIN {original.Password}";
            string result = $"{gettedPsws.Count} {gettedPsws[0].Service} {gettedPsws[0].Login} " +
                $"{gettedPsws[0].Password}";

            Assert.Equal(expected, result);
        }
    }
}