using System;
using System.Security.Cryptography;

namespace CodingApp.Entities {
    public static class ThreadSafeRandom {
        private static readonly RNGCryptoServiceProvider _cryptoProvider = new RNGCryptoServiceProvider();
        [ThreadStatic]
        private static Random _random;
        private static Random Random {
            get {
                if (_random == null) {
                    var buffer = new byte[4];
                    _cryptoProvider.GetBytes(buffer);
                    _random = new Random(BitConverter.ToInt32(buffer, 0));
                }
                return _random;
            }
        }

        public static double NextDouble() => Random.NextDouble();
    }
}
