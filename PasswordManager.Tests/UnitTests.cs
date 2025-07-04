using Newtonsoft.Json;
using PasswordManager.Crypto;
using PasswordManager.Data;
using static PasswordManager.Data.DataManager.JsonManager;
using static PasswordManager.Data.DataManager;
using PasswordManager.Core;
using System.Text;
using System.Security.Cryptography;
using System;

namespace PasswordManager.Tests
{
    public class UnitTests
    {
        private class TestData
        {
            private static List<PasswordEntry> testData;
            static TestData()
            {
                testData = new List<PasswordEntry>()
                {
                    new PasswordEntry()
                    {
                        ID = "1",
                        Service = "service_1",
                        Url = "service1.com",
                        Login = "login1",
                        Password = "psw1",
                        Category = "cat1"
                    },
                    new PasswordEntry()
                    {
                        ID = "2",
                        Service = "service_2",
                        Url = "service2.com",
                        Login = "login2",
                        Password = "psw2",
                        Category = "cat1"
                    },
                    new PasswordEntry()
                    {
                        ID = "3",
                        Service = "service_3",
                        Url = "service3.ru",
                        Login = "login3",
                        Password = "psw3",
                        Category = "cat2"
                    },
                    new PasswordEntry()
                    {
                        ID = "4",
                        Service = "service_4",
                        Url = "service4.com",
                        Login = "login4",
                        Password = "psw4",
                        Category = "cat2"
                    }
                };
            }
            public static List<PasswordEntry> GetTestData()
            {
                List<PasswordEntry> outputTestData = new List<PasswordEntry>();
                foreach (PasswordEntry password in testData)
                {
                    outputTestData.Add(new PasswordEntry()
                    {
                        ID = password.ID,
                        Service = password.Service,
                        Url = password.Url,
                        Login = password.Login,
                        Password = password.Password,
                        Category = password.Category
                    });
                }
                return outputTestData;
            }
        }

        // Модуль Crypto: CryptoManager
        // Протестирован функционал шифровки сообщения и его дешифровки
        [Fact]
        public void EncryptAndDecrypt_Message_ReturnsTheSameMessage()
        {
            string originalMessage = "Hello, world!";
            string password = "qwerty";
            byte[] salt = KeyManager.GenerateSalt();
            byte[] key = KeyManager.MakeKey(password, salt);
            byte[] messageAsByteArray = EncodeManager.MakeByteArr(originalMessage);

            byte[] encryptedData = CryptoManager.EncryptManager.EncryptData(messageAsByteArray, key);
            byte[] decryptedData = CryptoManager.DecryptManager.DecryptData(encryptedData, key);
            string comparedStr = EncodeManager.MakeStringFromByteArr(decryptedData);

            Assert.Equal(originalMessage, comparedStr);
        }

        [Fact]
        public void EncryptAndDecrypt_MessageAndNewSaltedKey_ReturnsException()
        {
            string originalMessage = "Hello, world!";
            string password = "qwerty";
            byte[] salt = KeyManager.GenerateSalt();
            byte[] key = KeyManager.MakeKey(password, salt);
            byte[] messageAsByteArray = EncodeManager.MakeByteArr(originalMessage);
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
            byte[] messageAsByteArray = EncodeManager.MakeByteArr(originalMessage);
            string incorrectPsw = "qvertu";
            byte[] newKey = KeyManager.MakeKey(incorrectPsw, salt);

            byte[] encryptedData = CryptoManager.EncryptManager.EncryptData(messageAsByteArray, key);

            var exception = Assert.Throws<CryptographicException>(
                () => CryptoManager.DecryptManager.DecryptData(encryptedData, newKey));
        }

        // Модуль Crypto: KeyManager
        // Протестирован функционал проверки соответствия хэшированного мастер-пароля
        [Fact]
        public void ComparePasswords_OriginalPswAndHashedOriginalPsw_ReturnsTrue()
        {
            string originalPsw = "_so-me_@Pass_word!";
            byte[] hashPsw = SHA256.HashData(Encoding.UTF8.GetBytes(originalPsw));

            bool result = KeyManager.ComparePasswords(originalPsw, hashPsw);

            Assert.True(result);
        }

        [Fact]
        public void ComparePasswords_OriginalPswAndHashedIncorrectPsw_ReturnsFalse()
        {
            string originalPsw = "_so-me_@Pass_word!";
            string incorrectPsw = "_so-me_@Pass_vord!";
            byte[] hashPsw = SHA256.HashData(Encoding.UTF8.GetBytes(originalPsw));

            bool result = KeyManager.ComparePasswords(incorrectPsw, hashPsw);

            Assert.False(result);
        }

        // Модуль Data: EncodeManager
        // Протестирован функционал перевода строки в массив байт и обратно
        [Fact]
        public void EncodeManagerMethods_Message_ReturnsTheSameMessage()
        {
            string message = "some_message123";

            string result = EncodeManager.MakeStringFromByteArr(
                EncodeManager.MakeByteArr(message));

            Assert.Equal(message, result);
        }

        // Модуль Core: MainProcess
        // Методы протестированы с помощью подхода Dependency Injection.
        // Файловую систему эмулирует специальный класс.
        [Fact]
        public void FindRepetition_NewEntry_ReturnsTupleOfEntryAndTrue()
        {
            FileManagerTests fileManager = new FileManagerTests();
            List<PasswordEntry> passwords = TestData.GetTestData();
            MainProcess mainProcess = new MainProcess("qwerty", fileManager, passwords);
            PasswordEntry newEntry = new PasswordEntry()
            {
                ID = "5",
                Service = "service_2",
                Url = "service2.com",
                Login = "login2",
                Password = "psw2",
                Category = "cat1"
            };
            
            var result = mainProcess.FindRepetition(newEntry);

            Assert.Equal(mainProcess.GetPasswords()[1], result.Value.Item1);
            Assert.True(result.Value.Item2);
        }

        [Fact]
        public void FindRepetition_NewEntry_ReturnsTupleOfEntryAndFalse()
        {
            FileManagerTests fileManager = new FileManagerTests();
            List<PasswordEntry> passwords = TestData.GetTestData();
            MainProcess mainProcess = new MainProcess("qwerty", fileManager, passwords);
            PasswordEntry newEntry = new PasswordEntry()
            {
                ID = "5",
                Service = "service_2",
                Url = "service2.com",
                Login = "login2",
                Password = "newPsw2",
                Category = "cat1"
            };

            var result = mainProcess.FindRepetition(newEntry);

            Assert.Equal(mainProcess.GetPasswords()[1], result.Value.Item1);
            Assert.False(result.Value.Item2);
        }

        [Fact]
        public void FindRepetition_NewEntry_ReturnsNull()
        {
            FileManagerTests fileManager = new FileManagerTests();
            List<PasswordEntry> passwords = TestData.GetTestData();
            MainProcess mainProcess = new MainProcess("qwerty", fileManager, passwords);
            PasswordEntry newEntry = new PasswordEntry()
            {
                ID = "5",
                Service = "service_2",
                Url = "service2.com",
                Login = "login5",
                Password = "psw2",
                Category = "cat1"
            };

            var result = mainProcess.FindRepetition(newEntry);

            Assert.Null(result);
        }

        [Fact]
        public void AddPassword_NewPassword_ReturnsListWithNewPassword()
        {
            FileManagerTests fileManager = new FileManagerTests();
            List<PasswordEntry> passwords = TestData.GetTestData();
            int expectedCount = passwords.Count + 1;
            MainProcess mainProcess = new MainProcess("qwerty", fileManager, passwords);
            PasswordEntry newEntry = new PasswordEntry()
            {
                ID = "5",
                Service = "service_5",
                Url = "service5.su",
                Login = "login5",
                Password = "psw5",
                Category = "cat2"
            };

            mainProcess.AddPassword(newEntry);
            List<PasswordEntry> resultList = mainProcess.GetPasswords();
            PasswordEntry resultEntry = resultList.Last();

            Assert.Equal(newEntry, resultEntry);
            Assert.Equal(expectedCount, resultList.Count);
        }

        [Fact]
        public void AddPassword_NewPasswordWithEmptyFields_ReturnsException()
        {
            FileManagerTests fileManager = new FileManagerTests();
            List<PasswordEntry> passwords = TestData.GetTestData();
            int expectedCount = passwords.Count + 1;
            MainProcess mainProcess = new MainProcess("qwerty", fileManager, passwords);
            PasswordEntry newEntry = new PasswordEntry()
            {
                ID = "5",
                Url = "",
                Login = "login5",
                Password = "psw5",
                Category = "cat2"
            };

            var exception = Assert.Throws<Exception>(() => mainProcess.AddPassword(newEntry));
            Assert.Equal("Required field are not filled in!", exception.Message);
        }

        [Fact]
        public void RemovePassword_PasswordToRemove_PasswordSuccessfullyRemoved()
        {
            int indexOfRemovedPassword = 1;
            FileManagerTests fileManager = new FileManagerTests();
            List<PasswordEntry> passwords = TestData.GetTestData();
            int expectedCount = passwords.Count - 1;
            MainProcess mainProcess = new MainProcess("qwerty", fileManager, passwords);

            mainProcess.RemovePassword(passwords[indexOfRemovedPassword]);
            List<PasswordEntry> result = mainProcess.GetPasswords();

            Assert.Equal(expectedCount, result.Count);
            Assert.False(result.Contains(TestData.GetTestData()[indexOfRemovedPassword]));
        }

        [Fact]
        public void ChangePassword_PasswordToChange_PasswordSuccessfullyChanged()
        {
            int indexOfChangedPassword = 2;
            FileManagerTests fileManager = new FileManagerTests();
            List<PasswordEntry> passwords = TestData.GetTestData();
            MainProcess mainProcess = new MainProcess("qwerty", fileManager, passwords);

            passwords[indexOfChangedPassword].Password = "newPsw";
            passwords[indexOfChangedPassword].Login = "newLogin";
            mainProcess.ChangePassword(passwords[indexOfChangedPassword]);
            PasswordEntry result = mainProcess.GetPasswords()[indexOfChangedPassword];

            Assert.Equal(passwords[indexOfChangedPassword], result);
        }

        [Fact]
        public void GetPasswords_PasswordsSuccessfullyReceived()
        {
            FileManagerTests fileManager = new FileManagerTests();
            List<PasswordEntry> passwords = TestData.GetTestData();
            MainProcess mainProcess = new MainProcess("qwerty", fileManager, passwords);

            List<PasswordEntry> gettedPasswords = mainProcess.GetPasswords();

            Assert.Equal(passwords, gettedPasswords);
        }

        [Fact]
        public void FilterCategories_ListOfCategories_FilteredEntries()
        {
            FileManagerTests fileManager = new FileManagerTests();
            List<PasswordEntry> passwords = TestData.GetTestData();
            MainProcess mainProcess = new MainProcess("qwerty", fileManager, passwords);
            List<string> categories = new List<string>() { "cat1", "cat2"};
            List<PasswordEntry> cat1List = new List<PasswordEntry>()
            {
                passwords[0],
                passwords[1]
            };
            List<PasswordEntry> cat2List = new List<PasswordEntry>()
            {
                passwords[2],
                passwords[3]
            };

            Dictionary<string, List<PasswordEntry>> result = 
                mainProcess.FilterCategories(categories);
            
            Assert.Equal(2, result.Count);
            Assert.Equal(2, result["cat1"].Count);
            Assert.Equal(2, result["cat2"].Count);
            Assert.Equal(cat1List, result["cat1"]);
            Assert.Equal(cat2List, result["cat2"]);
        }

        [Fact]
        public void GetAllCategories_AllCategoriesSuccessfullyReceived()
        {
            FileManagerTests fileManager = new FileManagerTests();
            List<PasswordEntry> passwords = TestData.GetTestData();
            MainProcess mainProcess = new MainProcess("qwerty", fileManager, passwords);
            List<string> categories = new List<string>() { "cat1", "cat2" };

            List<string> result = mainProcess.GetAllCategories();

            Assert.Equal(categories, result);
        }

        [Fact]
        public void AddAndSavePassword_NewPassword_AddAndSaveNewPassword()
        {
            FileManagerTests fileManager = new FileManagerTests();
            List<PasswordEntry> passwords = TestData.GetTestData();
            MainProcess mainProcess = new MainProcess("qwerty", fileManager, passwords);
            PasswordEntry newEntry = new PasswordEntry()
            {
                ID = "5",
                Service = "service_5",
                Url = "service5.su",
                Login = "login5",
                Password = "psw5",
                Category = "cat2"
            };

            mainProcess.AddPassword(newEntry);
            mainProcess.SavePasswords();
            mainProcess = new MainProcess("qwerty", fileManager);
            List<PasswordEntry> resultList = mainProcess.GetPasswords();

            Assert.Equal(passwords, resultList);
        }

        [Fact]
        public void RemovePasswordAndSave_PasswordToRemove_PasswordRemovedAndSaved()
        {
            int indexOfRemovedPassword = 1;
            FileManagerTests fileManager = new FileManagerTests();
            List<PasswordEntry> passwords = TestData.GetTestData();
            MainProcess mainProcess = new MainProcess("qwerty", fileManager, passwords);

            mainProcess.RemovePassword(passwords[indexOfRemovedPassword]);
            mainProcess.SavePasswords();
            mainProcess = new MainProcess("qwerty", fileManager);
            List<PasswordEntry> result = mainProcess.GetPasswords();

            Assert.Equal(passwords, result);
        }

        [Fact]
        public void ChangePasswordAndSave_PasswordToChange_PasswordChangedAndSaved()
        {
            int indexOfChangedPassword = 2;
            FileManagerTests fileManager = new FileManagerTests();
            List<PasswordEntry> passwords = TestData.GetTestData();
            MainProcess mainProcess = new MainProcess("qwerty", fileManager, passwords);

            passwords[indexOfChangedPassword].Password = "newPsw";
            passwords[indexOfChangedPassword].Login = "newLogin";
            mainProcess.ChangePassword(passwords[indexOfChangedPassword]);
            mainProcess.SavePasswords();
            mainProcess = new MainProcess("qwerty", fileManager);
            List<PasswordEntry> result = mainProcess.GetPasswords();

            Assert.Equal(passwords, result);
        }
    }
}