using System;
using System.Security.Cryptography;
using System.Text;

namespace TrayPasswordGenerator
{
    public static class PasswordGenerator
    {
        private const string LOWER        = "abcdefghijklmnopqrstuvwxyz";
        private const string UPPER        = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string DIGITS       = "0123456789";
        private const string SAFE_SPECIAL = "_-+@#";
        private const string ALL_SPECIAL  = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

        public static string CreatePassword(AppSettings s)
        {
            if (s.PasswordLength < s.StaticPrefix.Length)
                throw new ArgumentException("Длина пароля меньше длины префикса.");

            string alphabet = BuildAlphabet(s);
            if (alphabet.Length == 0)
                throw new InvalidOperationException("Не выбрана ни одна категория символов.");

            int need = s.PasswordLength - s.StaticPrefix.Length;
            char[] buf = new char[need];

            for (int i = 0; i < need; i++)
                buf[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];

            return s.StaticPrefix + new string(buf);
        }

        private static string BuildAlphabet(AppSettings s)
        {
            var sb = new StringBuilder(LOWER);
            if (s.UseUppercase) sb.Append(UPPER);
            if (s.UseNumbers)   sb.Append(DIGITS);

            sb.Append(s.SpecialCharactersMode switch
            {
                AppSettings.SpecialMode.Safe => SAFE_SPECIAL,
                AppSettings.SpecialMode.All  => ALL_SPECIAL,
                _                            => string.Empty
            });

            return sb.ToString();
        }
    }
}
