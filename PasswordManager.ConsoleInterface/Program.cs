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
            string masterPassword;
            string filePath = "psw.json";

            // Сначала проверяем первый ли у нас запуск.
            // Если первый, то просим придумать пароль.
            if (!MainProcess.GetRegStatus(filePath))
            {
                while (true)
                {
                    Console.Write("Придумайте мастер-пароль: ");
                    masterPassword = MainProcess.ReadPasswordConsole();
                    Console.Clear();
                    Console.Write("Повторите мастер-пароль: ");
                    string tempMasterPassword = MainProcess.ReadPasswordConsole();
                    Console.Clear();
                    if (tempMasterPassword == masterPassword)
                    {
                        Console.WriteLine("Мастер-пароль успешно установлен!");
                        Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
                        Console.ReadKey(intercept: true);
                        break;
                    }
                    else
                        Console.WriteLine("Мастер-пароли не совпадают, попробуйте еще раз!");
                    Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
                    Console.ReadKey(intercept: true);
                    Console.Clear();
                }
            }
            else
            {
                while (true)
                {
                    Console.Write("Введите мастер-пароль: ");
                    masterPassword = MainProcess.ReadPasswordConsole();
                    if (MainProcess.CheckMasterPassword(masterPassword, filePath))
                    {
                        Console.WriteLine("Авторизация прошла успешно!");
                        Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
                        Console.ReadKey(intercept: true);
                        Console.Clear();
                        break;
                    }
                    Console.WriteLine("Мастер-пароли не совпадают, попробуйте еще раз!");
                    Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
                    Console.ReadKey(intercept: true);
                    Console.Clear();
                }
            }
        }
    }
}
