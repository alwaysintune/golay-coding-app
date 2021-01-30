using CodingApp.Entities;

namespace CodingApp.Codes.Golay {
    public class Golay : IBinaryCode {
        public readonly BinaryMatrix GeneratorMatrix24;
        public readonly BinaryMatrix GeneratorMatrix23;
        public readonly BinaryMatrix ParityCheckMatrix24;
        public readonly BinaryMatrix MatrixB;
        private IGolayState _state;
        private const string _requestingRetransmission = "Found more than three errors. Requesting retransmission.";

        private GolayType _codingType;
        public GolayType CodingType {
            get => _codingType;
            set {
                _codingType = value;
                if (_codingType == GolayType.c23)
                    _state = new Golay23();
                else
                    _state = new Golay24();
            }
        }

        public BinaryMatrix GeneratorMatrix {
            get => CodingType == GolayType.c23 ?
                GeneratorMatrix23 : GeneratorMatrix24;
        }

        public Golay(GolayType golayType = GolayType.c23) {
            CodingType = golayType;
            GeneratorMatrix24 = new BinaryMatrix(12, 24);

            string data = "100000000000" + "110111000101" +
                          "010000000000" + "101110001011" +
                          "001000000000" + "011100010111" +
                          "000100000000" + "111000101101" +
                          "000010000000" + "110001011011" +
                          "000001000000" + "100010110111" +
                          "000000100000" + "000101101111" +
                          "000000010000" + "001011011101" +
                          "000000001000" + "010110111001" +
                          "000000000100" + "101101110001" +
                          "000000000010" + "011011100011" +
                          "000000000001" + "111111111110";

            for (int i = 0; i < GeneratorMatrix24.M; i++) {
                for (int j = 0; j < GeneratorMatrix24.N; j++) {
                    GeneratorMatrix24[i, j] = (byte)char.GetNumericValue(data[i * GeneratorMatrix24.N + j]);
                }
            }

            ParityCheckMatrix24 = GeneratorMatrix24.Transpose();
            GeneratorMatrix23 = GeneratorMatrix24.Subspace(0, 0, 12, 23);
            MatrixB = GeneratorMatrix24.Subspace(0, 12, 12, 12);
        }

        public BinaryMatrix Encode(BinaryMatrix matrix) {
            return _state.Encode(this, matrix);
        }

        public BinaryMatrix Decode(BinaryMatrix receivedWord) {
            try {
                return _state.Decode(receivedWord, FoundErrorPattern);
            }
            catch (System.Exception ex) {
                throw new System.Exception(ex.Message + ": " + _requestingRetransmission);
            }
        }

        private bool FoundErrorPattern(BinaryMatrix receivedWord, ref BinaryMatrix errorPattern) {
            var syndrome1 = receivedWord * ParityCheckMatrix24;

            if (syndrome1.Weight() <= 3) {
                errorPattern = new BinaryMatrix(syndrome1, new BinaryMatrix(1, syndrome1.N));
                return true;
            }

            for (int i = 0; i < MatrixB.M; i++) {
                var row = MatrixB.Row(i);

                if ((syndrome1 + row).Weight() <= 2) {
                    var eRow = new BinaryMatrix(1, MatrixB.N);
                    eRow[0, i] = 1;
                    errorPattern = new BinaryMatrix(syndrome1 + row, eRow);
                    return true;
                }
            }

            var syndrome2 = syndrome1 * MatrixB;

            if (syndrome2.Weight() <= 3) {
                errorPattern = new BinaryMatrix(new BinaryMatrix(1, syndrome2.N), syndrome2);
                return true;
            }

            for (int i = 0; i < MatrixB.M; i++) {
                var row = MatrixB.Row(i);

                if ((syndrome2 + row).Weight() <= 2) {
                    var eRow = new BinaryMatrix(1, MatrixB.N);
                    eRow[0, i] = 1;
                    errorPattern = new BinaryMatrix(eRow, syndrome2 + row);
                    return true;
                }
            }

            return false;
        }
    }
}
