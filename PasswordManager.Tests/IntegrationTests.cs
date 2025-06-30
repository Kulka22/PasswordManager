using PasswordManager.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Tests
{
    public class IntegrationTests
    {
        // Тест для метода проверки корректности введенного пароля
        [Fact]
        public void SaveDataAndCheckMasterPassword_MasterPassword_ReturnsTrue()
        {
            string tempFile = "CheckPasswordTest.json";
            string password = MainProcess.GeneratePassword(16);
            MainProcess main = new MainProcess(password, tempFile);

            main.SavePasswords();
            bool result = MainProcess.CheckMasterPassword(password, tempFile);

            Assert.True(result);
            
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }


    }
}
