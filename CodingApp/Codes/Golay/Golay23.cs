using CodingApp.Entities;

namespace CodingApp.Codes.Golay {
    class Golay23 : IGolayState {
        public BinaryMatrix Encode(Golay golay, BinaryMatrix matrix) {
            return matrix * golay.GeneratorMatrix23;
        }

        public BinaryMatrix Decode(BinaryMatrix receivedWord, FoundErrorPattern foundErrorPattern) {
            var digit = new BinaryMatrix(1, 1);
            if (receivedWord.Weight() % 2 == 0)
                digit[0, 0] = 1;

            receivedWord = new BinaryMatrix(receivedWord, digit);

            var errorPattern = new BinaryMatrix();
            if (foundErrorPattern(receivedWord, ref errorPattern)) {
                var result = receivedWord + errorPattern;

                return result.Subspace(0, 0, 1, 23);
            }

            throw new System.Exception("C23");
        }
    }
}
