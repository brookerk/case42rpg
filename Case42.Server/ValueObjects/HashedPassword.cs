using System;
using Case42.Base.Extensions;
using System.Security.Cryptography;

namespace Case42.Server.ValueObjects
{
    public class HashedPassword
    {
        public string Hash { get; private set; }
        public string Salt { get; private set; }

        public HashedPassword(string hash, string salt)
        {
            Hash = hash;
            Salt = salt;
        }

        /// <summary>
        /// for nhibernate
        /// </summary>
        private HashedPassword()
        {
        }


        public static HashedPassword fromPlainText(string password)
        {
            var random = new RNGCryptoServiceProvider();
            var bytes = new byte[16];
            random.GetBytes(bytes);
            var salt = bytes.ToHexString();

            return new HashedPassword((password + salt).ToSha1(), salt);
        }

        public bool EqualsPlainText(string plainText)
        {
            return (plainText + Salt).ToSha1() == Hash;
        }
    }
}
