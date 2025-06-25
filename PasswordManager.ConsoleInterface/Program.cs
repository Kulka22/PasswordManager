using Newtonsoft.Json;
using PasswordManager.Core;
using PasswordManager.Crypto;
using PasswordManager.Data;
using static PasswordManager.Data.DataManager.JsonManager;

namespace PasswordManager.ConsoleInterface
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainProcess main = new MainProcess("qwerty78", "console.json");
            main.SavePasswords();
            bool result = MainProcess.CheckMasterPassword("qwerty78", "console.json");
            Console.WriteLine(result);
        }
    }
}
