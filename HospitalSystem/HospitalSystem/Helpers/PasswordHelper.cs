namespace HospitalSystem.Helpers
{
    public static class PasswordHelper
    {
        public static string Hash(string plainPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainPassword);
        }

        // Понимает и новые bcrypt-хеши, и старые пароли в открытом виде —
        // это то, что позволяет существующим учёткам не сломаться при переходе
        public static bool Verify(string plainPassword, string storedValue)
        {
            if (IsBcryptHash(storedValue))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(plainPassword, storedValue);
                }
                catch
                {
                    // Повреждённое или нераспознанное значение — считаем,
                    // что пароль не подошёл, а не роняем приложение
                    return false;
                }
            }

            // Старый формат — пароль в базе лежит как есть
            return storedValue == plainPassword;
        }

        // Bcrypt-хеш всегда начинается с "$2" и имеет длину ровно 60 символов —
        // этого достаточно, чтобы отличить его от старого открытого пароля
        public static bool IsBcryptHash(string value)
        {
            return !string.IsNullOrEmpty(value) && value.StartsWith("$2") && value.Length == 60;
        }
    }
}