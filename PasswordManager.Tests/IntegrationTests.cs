using Newtonsoft.Json;
using PasswordManager.Crypto;
using PasswordManager.Data;
using static PasswordManager.Data.DataManager.JsonManager;
using static PasswordManager.Data.DataManager;
using PasswordManager.Core;
using System.Text;
using System.Security.Cryptography;
using static PasswordManager.Data.DataManager;
using System.Reflection;

namespace PasswordManager.Tests
{
    public class IntegrationTests
    {
        public readonly string filePath = "testFile.json";
        public readonly string masterPassword = "qwerty";
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

        // Тест для метода проверки корректности введенного пароля
        [Fact]
        public void SaveDataAndCheckMasterPassword_MasterPassword_ReturnsTrue()
        {
            string tempFile = "CheckPasswordTest.json";

            if (File.Exists(tempFile))
                File.Delete(tempFile);

            string password = MainProcess.GeneratePassword(16);
            MainProcess main = new MainProcess(password, null, null, tempFile);

            main.SavePasswords();
            bool result = MainProcess.CheckMasterPassword(password, tempFile);

            Assert.True(result);

            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }

        [Fact]
        public void SavePassword_NewPasswords_SaveNewPasswords()
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            FileManager fileManager = new FileManager();
            List<PasswordEntry> passwords = TestData.GetTestData();

            MainProcess mainProcess = new MainProcess(masterPassword, fileManager,
                passwords, filePath);
            mainProcess.SavePasswords();
            mainProcess = new MainProcess("qwerty", fileManager, null, filePath);
            List<PasswordEntry> resultList = mainProcess.GetPasswords();

            Assert.Equal(passwords, resultList);
        }

        [Fact]
        public void AddAndSavePassword_NewPassword_AddAndSaveNewPassword()
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            FileManager fileManager = new FileManager();
            List<PasswordEntry> passwords = TestData.GetTestData();
            MainProcess mainProcess = new MainProcess(masterPassword, fileManager, 
                passwords, filePath);
            mainProcess.SavePasswords();
            Assert.True(MainProcess.CheckMasterPassword(masterPassword, filePath));
            mainProcess = new MainProcess("qwerty", fileManager, null, filePath);
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
            mainProcess = new MainProcess("qwerty", fileManager, null, filePath);
            List<PasswordEntry> resultList = mainProcess.GetPasswords();

            Assert.Equal(newEntry, resultList.Last());
            Assert.Equal(passwords.Count + 1, resultList.Count);
        }

        [Fact]
        public void RemovePasswordAfterAdd_PasswordToRemove_PasswordRemovedAndSaved()
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            int indexOfRemovedPassword = 1;
            FileManager fileManager = new FileManager();
            List<PasswordEntry> passwords = TestData.GetTestData();
            MainProcess mainProcess = new MainProcess(masterPassword, fileManager,
                null, filePath);
            foreach (PasswordEntry password in passwords)
                mainProcess.AddPassword(password);
            mainProcess.SavePasswords();
            Assert.True(MainProcess.CheckMasterPassword(masterPassword, filePath));
            mainProcess = new MainProcess("qwerty", fileManager, null, filePath);
            PasswordEntry removedPassword = mainProcess.GetPasswords()[indexOfRemovedPassword];

            mainProcess.RemovePassword(removedPassword);
            mainProcess.SavePasswords();
            mainProcess = new MainProcess("qwerty", fileManager, null, filePath);
            List<PasswordEntry> result = mainProcess.GetPasswords();

            Assert.True(!result.Contains(removedPassword));
            Assert.Equal(passwords.Count - 1, result.Count);
        }

        [Fact]
        public void RemovePasswordAndSave_PasswordToRemove_PasswordRemovedAndSaved()
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            int indexOfRemovedPassword = 1;
            FileManager fileManager = new FileManager();
            List<PasswordEntry> passwords = TestData.GetTestData();
            MainProcess mainProcess = new MainProcess(masterPassword, fileManager,
                passwords, filePath);
            mainProcess.SavePasswords();
            Assert.True(MainProcess.CheckMasterPassword(masterPassword, filePath));
            mainProcess = new MainProcess("qwerty", fileManager, null, filePath);

            mainProcess.RemovePassword(passwords[indexOfRemovedPassword]);
            mainProcess.SavePasswords();
            mainProcess = new MainProcess("qwerty", fileManager, null, filePath);
            List<PasswordEntry> result = mainProcess.GetPasswords();

            Assert.DoesNotContain(passwords[indexOfRemovedPassword], result);
            Assert.Equal(passwords.Count - 1, result.Count);
        }

        [Fact]
        public void ChangePasswordAndSave_PasswordToChange_PasswordChangedAndSaved()
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            int indexOfChangedPassword = 2;
            FileManager fileManager = new FileManager();
            List<PasswordEntry> passwords = TestData.GetTestData();
            MainProcess mainProcess = new MainProcess(masterPassword, fileManager,
                passwords, filePath);
            mainProcess.SavePasswords();
            Assert.True(MainProcess.CheckMasterPassword(masterPassword, filePath));
            mainProcess = new MainProcess("qwerty", fileManager, null, filePath);

            passwords[indexOfChangedPassword].Password = "newPsw";
            passwords[indexOfChangedPassword].Login = "newLogin";
            mainProcess.ChangePassword(passwords[indexOfChangedPassword]);
            mainProcess.SavePasswords();
            mainProcess = new MainProcess("qwerty", fileManager, null, filePath);
            List<PasswordEntry> result = mainProcess.GetPasswords();

            Assert.Equal(passwords, result);
        }
    }
}
