namespace Plus.Communication.Encryption.KeyExchange
{
    using System;
    using Utilities;

    public sealed class DiffieHellman
    {
        public readonly int BITLENGTH = 32;

        private BigInteger PrivateKey;

        public DiffieHellman()
        {
            Initialize();
        }

        public DiffieHellman(int b)
        {
            BITLENGTH = b;
            Initialize();
        }

        public DiffieHellman(BigInteger prime, BigInteger generator)
        {
            Prime = prime;
            Generator = generator;
            Initialize(true);
        }

        public BigInteger Prime { get; private set; }
        public BigInteger Generator { get; private set; }
        public BigInteger PublicKey { get; private set; }

        private void Initialize(bool ignoreBaseKeys = false)
        {
            PublicKey = 0;
            var rand = new Random();
            while (PublicKey == 0)
            {
                if (!ignoreBaseKeys)
                {
                    Prime = BigInteger.genPseudoPrime(BITLENGTH, 10, rand);
                    Generator = BigInteger.genPseudoPrime(BITLENGTH, 10, rand);
                }
                var bytes = new byte[BITLENGTH / 8];
                Randomizer.NextBytes(bytes);
                PrivateKey = new BigInteger(bytes);
                if (Generator > Prime)
                {
                    var temp = Prime;
                    Prime = Generator;
                    Generator = temp;
                }
                PublicKey = Generator.modPow(PrivateKey, Prime);
                if (!ignoreBaseKeys)
                {
                    break;
                }
            }
        }

        public BigInteger CalculateSharedKey(BigInteger m) => m.modPow(PrivateKey, Prime);
    }
}