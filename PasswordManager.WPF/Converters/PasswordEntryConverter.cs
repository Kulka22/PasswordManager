using PasswordManager.WPF.Models;

namespace PasswordManager.WPF.Converters
{
    public static class PasswordEntryConverter
    {
        public static PasswordEntry ToLocal(Data.DataManager.JsonManager.PasswordEntry externalEntry)
        {
            if (externalEntry == null) return null;

            return new PasswordEntry
            {
                ID = externalEntry.ID,
                Service = externalEntry.Service ?? string.Empty,
                Category = externalEntry.Category ?? string.Empty,
                Url = externalEntry.Url ?? string.Empty,
                Login = externalEntry.Login ?? string.Empty,
                Password = externalEntry.Password ?? string.Empty
            };
        }

        public static Data.DataManager.JsonManager.PasswordEntry ToExternal(PasswordEntry localEntry)
        {
            if (localEntry == null) return null;
            return new Data.DataManager.JsonManager.PasswordEntry
            {
                ID = localEntry.ID,
                Service = localEntry.Service ?? string.Empty,
                Category = localEntry.Category ?? string.Empty,
                Url = localEntry.Url ?? string.Empty,
                Login = localEntry.Login ?? string.Empty,
                Password = localEntry.Password ?? string.Empty
            };
        }
    }
}