using CodingApp.Entities;

namespace CodingApp.Codes {
    public interface IBinaryCode {
        BinaryMatrix Encode(BinaryMatrix matrix);
        BinaryMatrix Decode(BinaryMatrix receivedWord);
        BinaryMatrix GeneratorMatrix { get; }
    }
}
