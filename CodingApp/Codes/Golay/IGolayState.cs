using CodingApp.Entities;

namespace CodingApp.Codes.Golay {
    public delegate bool FoundErrorPattern(BinaryMatrix receivedWord, ref BinaryMatrix errorPattern);
    public interface IGolayState {
        BinaryMatrix Encode(Golay golay, BinaryMatrix matrix);
        BinaryMatrix Decode(BinaryMatrix receivedWord, FoundErrorPattern foundErrorPattern);
    }
}
