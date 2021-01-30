using CodingApp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodingApp.Auxiliary.Internal {
    public static class DataConverter {
        public static BinaryMatrix StringToVector(string data, int vectorLength) {
            if (data.Length != vectorLength)
                throw new Exception("Vector's length is not equal to " + vectorLength + ".");

            var vector = new BinaryMatrix(1, vectorLength);

            for (int i = 0; i < vector.M; i++) {
                for (int j = 0; j < vector.N; j++) {
                    vector[i, j] = (byte)char.GetNumericValue(data[i * vector.N + j]);
                }
            }

            return vector;
        }

        public static string StringToBinary(string str, Encoding encoding) {
            return ByteArrayToBinary(encoding.GetBytes(str));
        }

        public static string ByteArrayToBinary(byte[] arr) {
            return string.Join(string.Empty,
                arr
                .Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0'))
            );
        }

        public static string BinaryToString(string str, Encoding encoding) {
            return encoding.GetString(BinaryToByteArray(str));
        }

        public static byte[] BinaryToByteArray(string str) {
            return Regex.Split(str, "(.{8})")
                        .Where(binary => !string.IsNullOrEmpty(binary))
                        .Select(binary => Convert.ToByte(binary, 2))
                        .ToArray();
        }

        public static IEnumerable<string> Split(string str, int chunkSize) {
            if (chunkSize > str.Length)
                chunkSize = str.Length;

            return str == null || str == string.Empty || chunkSize <= 0 ?
                Enumerable.Empty<string>() :
                Enumerable.Range(0, (int)Math.Ceiling(str.Length / (double)chunkSize))
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }
    }
}
