namespace PasswordManager.Core
{
    public class Core
    {
        private static PasswordEntry _passwords;
        private static readonly string _filePath = "psw.json";
        public static byte[] _salt;
        public class PasswordEntry
        {
            public string Service { get; set; }
            public string Url { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
        }

        //public class VaultFile
        //{
        //    public string MetaData { get; set; }
        //    public string Entries { get; set; }
        //}

        //public class MetaDataFile
        //{
        //    public string Salt { get; set; }
        //    public string MasterPasswordHash { get; set; }
        //}
    }
}
