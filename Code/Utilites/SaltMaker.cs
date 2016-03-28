using System;
using System.Security.Cryptography;
using System.Text;

namespace ParserCore.Utilites
{
    /// <summary>
    /// Class for making new salt
    /// </summary>
    internal class SaltMaker
    {
        /// <summary>
        /// Implementation of some sort of random number generator
        /// </summary>
        private RandomNumberGenerator Generator;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="generator"></param>
        public SaltMaker(RandomNumberGenerator generator)
        {
            if (generator == null)
                throw new ArgumentNullException("generator");

            this.Generator = generator;
        }

        /// <summary>
        /// Generate new salt
        /// </summary>
        public string GetNewSalt(int length)
        {
            if (length <= 0)
                throw new ArgumentException("length <= 0");

            // set password hash and salt for user
            byte[] bytesStore = new byte[length];
            this.Generator.GetNonZeroBytes(bytesStore);
            string result = UTF8Encoding.Default.GetString(bytesStore);

            return result;
        }
    }
}