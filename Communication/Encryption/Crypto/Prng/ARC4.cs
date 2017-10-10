﻿namespace Plus.Communication.Encryption.Crypto.Prng
{
    public sealed class ARC4
    {
        public const int POOLSIZE = 256;
        private readonly byte[] bytes;
        private int i;
        private int j;

        public ARC4() => bytes = new byte[POOLSIZE];

        public ARC4(byte[] key)
        {
            bytes = new byte[POOLSIZE];
            Initialize(key);
        }

        public void Initialize(byte[] key)
        {
            i = 0;
            j = 0;
            for (i = 0; i < POOLSIZE; ++i)
            {
                bytes[i] = (byte) i;
            }
            for (i = 0; i < POOLSIZE; ++i)
            {
                j = (j + bytes[i] + key[i % key.Length]) & (POOLSIZE - 1);
                Swap(i, j);
            }

            i = 0;
            j = 0;
        }

        private void Swap(int a, int b)
        {
            var t = bytes[a];
            bytes[a] = bytes[b];
            bytes[b] = t;
        }

        public byte Next()
        {
            i = ++i & (POOLSIZE - 1);
            j = (j + bytes[i]) & (POOLSIZE - 1);
            Swap(i, j);
            return bytes[(bytes[i] + bytes[j]) & 255];
        }

        public void Encrypt(ref byte[] src)
        {
            for (var k = 0; k < src.Length; k++)
            {
                src[k] ^= Next();
            }
        }

        public void Decrypt(ref byte[] src)
        {
            Encrypt(ref src);
        }
    }
}