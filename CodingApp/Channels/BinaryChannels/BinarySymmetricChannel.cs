using CodingApp.Auxiliary.Internal;
using CodingApp.Codes;
using CodingApp.Entities;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodingApp.Channels.BinaryChannels {
    public class BinarySymmetricChannel {
        private readonly BitIterator _bitIterator;
        private readonly object _lockA = new object();
        private readonly object _lockB = new object();

        public int PaddingCount { get; set; }

        public int VectorLength { get; private set; }

        private IBinaryCode _binaryCode;
        public IBinaryCode BinaryCode {
            get => _binaryCode;
            set {
                _binaryCode = value;
                VectorLength = _binaryCode.GeneratorMatrix.M;
            }
        }

        public BinarySymmetricChannel(IBinaryCode binaryCode) {
            _bitIterator = new BitIterator();
            _binaryCode = binaryCode;
            VectorLength = _binaryCode.GeneratorMatrix.M;
        }
        public Task<string> SendWithoutEncoding(IEnumerable<string> textChunks, int length, double probability) {
            return Task.Run(() => {
                var builder = new StringBuilder(length);
                var vectors = new SortedDictionary<long, BinaryMatrix>();

                Parallel.ForEach(textChunks, (tc, state, index) => {
                    BinaryMatrix vector;

                    vector = DataConverter.StringToVector(tc, VectorLength);
                    vector = SendThroughChannel(vector, probability);

                    lock (_lockA) {
                        vectors.Add(index, vector);
                    }
                });

                foreach (var v in vectors) {
                    builder.Append(v.Value.ToString().Substring(0, VectorLength));
                }

                return builder.ToString().Substring(0, builder.Length - PaddingCount);
            });
        }

        public Task<byte[]> SendWithoutEncoding(byte[] pixels, int nearestMultiple, double probability) {
            return Task.Run(() => {
                var vectors = new BinaryMatrix[nearestMultiple / VectorLength];
                _bitIterator.ByteArray = pixels;

                Parallel.For(0, vectors.Length, (i) => {
                    var vector = new BinaryMatrix(1, VectorLength);
                    for (int j = 0; j < VectorLength; j++) {
                        vector[0, j] = _bitIterator[i * VectorLength + j];
                    }
                    vectors[i] = SendThroughChannel(vector, probability);
                });

                Parallel.For(0, vectors.Length, (i) => {
                    BinaryMatrix vector = vectors[i];
                    for (int j = 0; j < VectorLength; j++) {
                        _bitIterator[i * VectorLength + j] = vector[0, j];
                    }
                });

                return _bitIterator.ByteArray;
            });
        }

        public Task<string> SendWithEncoding(IEnumerable<string> textChunks, int length, double probability) {
            return Task.Run(() => {
                var builder = new StringBuilder(length);
                var vectors = new SortedDictionary<long, BinaryMatrix>();

                Parallel.ForEach(textChunks, (tc, state, index) => {
                    BinaryMatrix codeword;

                    codeword = EncodeInput(tc, VectorLength);
                    BinaryMatrix vector = SendThroughChannel(codeword, probability);
                    vector = _binaryCode.Decode(vector);

                    lock (_lockB) {
                        vectors.Add(index, vector);
                    }
                });

                foreach (var v in vectors) {
                    builder.Append(v.Value.ToString().Substring(0, VectorLength));
                }

                return builder.ToString().Substring(0, builder.Length - PaddingCount);
            });
        }

        public Task<byte[]> SendWithEncoding(byte[] pixels, int nearestMultiple, double probability) {
            return Task.Run(() => {
                var vectors = new BinaryMatrix[nearestMultiple / VectorLength];
                _bitIterator.ByteArray = pixels;

                Parallel.For(0, vectors.Length, (i) => {
                    var vector = new BinaryMatrix(1, VectorLength);
                    for (int j = 0; j < VectorLength; j++) {
                        vector[0, j] = _bitIterator[i * VectorLength + j];
                    }

                    vector = _binaryCode.Encode(vector);
                    vector = SendThroughChannel(vector, probability);
                    vector = _binaryCode.Decode(vector);

                    vectors[i] = vector;
                });

                Parallel.For(0, vectors.Length, (i) => {
                    BinaryMatrix vector = vectors[i];
                    for (int j = 0; j < VectorLength; j++) {
                        _bitIterator[i * VectorLength + j] = vector[0, j];
                    }
                });

                return _bitIterator.ByteArray;
            });
        }

        public BinaryMatrix SendThroughChannel(BinaryMatrix vector, double probability) {
            BinaryMatrix result = vector.Clone();
            for (int i = 0; i < vector.M; i++) {
                for (int j = 0; j < vector.N; j++) {
                    if (ThreadSafeRandom.NextDouble() < probability) {
                        result[i, j] = (byte)(vector[i, j] == 0 ? 1 : 0);
                    }
                }
            }

            return result;
        }

        public BinaryMatrix EncodeInput(string data, int vectorLength) {
            BinaryMatrix vector = DataConverter.StringToVector(data, vectorLength);

            return BinaryCode.Encode(vector);
        }
    }
}
