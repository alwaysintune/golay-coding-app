namespace CodingApp.Entities {
    public class BitIterator {
        public byte[] ByteArray { get; set; }
        public int Length {
            get => ByteArray.Length * 8;
        }
        public byte this[int i] {
            get => GetBit(i);
            set => SetBit(value, i);
        }

        public BitIterator() {
            ByteArray = new byte[] { 0x00 };
        }

        public BitIterator(byte[] array) {
            ByteArray = array;
        }

        private byte GetBit(int i) {
            if (i < 0 || i >= Length)
                return 0xFF;

            int number = ByteArray[i / 8];
            int remainder = 7 - i % 8;

            int mask = 0x01;
            mask <<= remainder;
            number &= mask;

            return (byte)(number >> remainder);
        }

        private void SetBit(int value, int i) {
            if (i < 0 || i >= Length)
                return;
            else if (value != 0)
                value = 0x01;

            int index = i / 8;
            int number = ByteArray[index];
            int remainder = 7 - i % 8;

            int mask = 0x01;
            mask <<= remainder;
            mask = ~mask;
            number &= mask;

            value <<= remainder;
            number |= value;

            ByteArray[index] = (byte)number;
        }
    }
}
