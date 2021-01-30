using CodingApp.Entities;

namespace CodingApp.Codes.Golay {
    class Golay24 : IGolayState {
        public BinaryMatrix Encode(Golay golay, BinaryMatrix matrix) {
            return matrix * golay.GeneratorMatrix24;
        }
        public BinaryMatrix Decode(BinaryMatrix receivedWord, FoundErrorPattern foundErrorPattern) {
            var errorPattern = new BinaryMatrix();
            if (foundErrorPattern(receivedWord, ref errorPattern)) {
                var result = receivedWord + errorPattern;

                return result;
            }

            throw new System.Exception("C24");
        }
    }
}
