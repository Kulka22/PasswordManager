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
            string originalMessage = "Hello, world!";
            string psw = "qwerty";
            byte[] salt = KeyManager.GenerateSalt();
            byte[] key = KeyManager.MakeKey(psw, salt);
            byte[] messageAsByteArray = DataManager.EncodeManager.MakeByteArr(originalMessage);
            byte[] newSalt = KeyManager.GenerateSalt();
            byte[] newKey = KeyManager.MakeKey(psw, newSalt);

            byte[] encryptedData = CryptoManager.EncryptManager.EncryptData(messageAsByteArray, key);
            byte[] decryptedData = CryptoManager.DecryptManager.DecryptData(encryptedData, newKey);

            string masterPassword;
            string filePath = "psw.json";
            List<PasswordEntry> passwords = new List<PasswordEntry>();

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

            MainProcess main = new MainProcess(masterPassword);
            passwords = main.GetPasswords();

            string inputButton;
            do
            {
                Console.WriteLine("Доступные действия");
                Console.WriteLine("1 - Вывод всех паролей.");
                Console.WriteLine("2 - Изменить пароль.");
                Console.WriteLine("3 - Удалить пароль.");
                Console.WriteLine("4 - Добавить пароль.");
                Console.WriteLine("e - Выход из программы.");
                Console.Write("Выберите действие: ");
                inputButton = Console.ReadLine();
                Console.Clear();
                switch (inputButton)
                {
                    case "1":
                        {
                            int num = 1;
                            foreach (PasswordEntry password in passwords)
                            {
                                Console.Write($"{num++}. {password.Service} " +
                                    $"{password.Login} ");
                                Console.Write(new string('*', password.Password.Length) + ";\n");
                            }
                            Console.WriteLine("Сделать пароли видимыми (1 - да, 0 - нет)?");
                            Console.Write("Ваш выбор: ");
                            string temp = Console.ReadLine();
                            if (temp == "1")
                            {
                                num = 1;
                                foreach (PasswordEntry password in passwords)
                                {
                                    Console.Write($"{num++}. {password.Service} " +
                                        $"{password.Login} {password.Password}\n");
                                }
                            }
                            else if (temp != "0")
                                Console.WriteLine("Некорректный ввод!");
                            break;
                        }
                    case "2":
                        {

                            break;
                        }
                    case "3":
                        {

                            break;
                        }
                    case "4":
                        {
                            string service, url, login, password;
                            Console.Write("Введите название сервиса: ");
                            service = Console.ReadLine();
                            Console.Write("Введите URL: ");
                            url = Console.ReadLine();
                            Console.Write("Введите логин: ");
                            login = Console.ReadLine();
                            Console.Write("Введите пароль: ");
                            password = MainProcess.ReadPasswordConsole();
                            main.AddPassword(new PasswordEntry() { Service = service, 
                            Url = url, Login = login, Password = password});
                            main.SavePasswords();
                            break;
                        }
                    case "e":
                        {

                            break;
                        }
                    default:
                        break;
                }
                Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
                Console.ReadKey(intercept: true);
                Console.Clear();
            } while (true);
        }
    }
}
