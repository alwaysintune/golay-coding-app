namespace CodingApp.Entities {
    public readonly struct BinaryMatrix {
        public readonly int M;
        public readonly int N;
        public readonly int Count;
        private readonly byte[] _byteArray;
        private const string _additiveOperationError = "Two matrices must have an equal number of rows and columns to be added.";
        private const string _multiplicativeOperationError = "The number of columns in the first matrix must be equal to the number of rows in the second matrix.";
        private const string _appendingOperationError = "To append two matrices both must have at least an equal number of rows.";
        private const string _settingElementError = "Invalid input. Available values are 0 or 1 within defined range of rows and columns.";

        public byte this[int i, int j] {
            get => GetElement(i, j);
            set => SetElement(value, i, j);
        }

        public BinaryMatrix(int rowsM, int columnsN) : this() {
            M = rowsM;
            N = columnsN;
            Count = M * N;
            _byteArray = new byte[MultipleOf(Count) / 8];
        }

        public BinaryMatrix(BinaryMatrix leftMatrix, BinaryMatrix rightMatrix) : this() {
            if (leftMatrix.M != rightMatrix.M)
                throw new System.Exception(_appendingOperationError);

            M = leftMatrix.M;
            N = leftMatrix.N + rightMatrix.N;
            Count = M * N;
            _byteArray = new byte[MultipleOf(Count) / 8];

            for (int i = 0; i < M; i++) {
                for (int j = 0; j < N; j++) {
                    this[i, j] = (j < leftMatrix.N) ?
                        leftMatrix[i, j] :
                        rightMatrix[i, j - leftMatrix.N];
                }
            }
        }

        public int Weight() {
            int weight = 0;
            for (int i = 0; i < M; i++) {
                for (int j = 0; j < N; j++) {
                    if (this[i, j] == 0x01)
                        weight++;
                }
            }

            return weight;
        }

        public BinaryMatrix Transpose() {
            var result = new BinaryMatrix(N, M);
            for (int i = 0; i < N; i++) {
                for (int j = 0; j < M; j++) {
                    result[i, j] = this[j, i];
                }
            }

            return result;
        }

        public BinaryMatrix Subspace(int i, int j, int rowsM, int columnsN) {
            if (!IsGreaterThan(-1, i, j, rowsM, columnsN) ||
                i + rowsM > M || j + columnsN > N)
                return this;

            var result = new BinaryMatrix(rowsM, columnsN);

            for (int x = 0; x < result.M; x++) {
                int k = j;
                for (int y = 0; y < result.N; y++) {
                    result[x, y] = this[i, k++];
                }
                i++;
            }

            return result;
        }

        private bool IsGreaterThan(int value, params int[] args) {
            foreach (var arg in args) {
                if (arg <= value)
                    return false;
            }

            return true;
        }

        public BinaryMatrix Row(int index) {
            var result = new BinaryMatrix(1, N);

            for (int j = 0; j < result.N; j++)
                result[0, j] = this[index, j];

            return result;
        }

        public BinaryMatrix Column(int index) {
            var result = new BinaryMatrix(M, 1);

            for (int i = 0; i < result.M; i++)
                result[i, 0] = this[i, index];

            return result;
        }

        public static BinaryMatrix operator +(BinaryMatrix op1, BinaryMatrix op2) {
            if (op1.M != op2.M || op1.N != op2.N)
                throw new System.Exception(_additiveOperationError);

            var result = new BinaryMatrix(op1.M, op1.N);

            for (int i = 0; i < op1.M; i++) {
                for (int j = 0; j < op1.N; j++) {
                    result[i, j] = (byte)((op1[i, j] + op2[i, j]) % 2);
                }
            }

            return result;
        }

        public static BinaryMatrix operator *(BinaryMatrix op1, BinaryMatrix op2) {
            if (op1.N != op2.M)
                throw new System.Exception(_multiplicativeOperationError);

            var result = new BinaryMatrix(op1.M, op2.N);
            for (int i = 0; i < result.M; i++) {
                for (int j = 0; j < result.N; j++) {
                    int dotProduct = 0;
                    for (int y = 0; y < op2.M; y++) {
                        dotProduct = (dotProduct + op1[i, y] * op2[y, j]) % 2;
                    }
                    result[i, j] = (byte)dotProduct;
                }
            }

            return result;
        }

        public bool CanBeAddedWith(BinaryMatrix matrix) {
            if (M != matrix.M || N != matrix.N)
                return false;

            return true;
        }

        public bool CanBeMultipliedWith(BinaryMatrix matrix) {
            if (N != matrix.M)
                return false;

            return true;
        }

        public override string ToString() {
            var result = new System.Text.StringBuilder();
            for (int i = 0; i < M; i++) {
                for (int j = 0; j < N; j++) {
                    result.Append(System.Convert.ToString(this[i, j], 2));
                }
            }

            return result.ToString();
        }

        public BinaryMatrix Clone() {
            var result = new BinaryMatrix(M, N);
            for (int i = 0; i < result.M; i++) {
                for (int j = 0; j < result.N; j++) {
                    result[i, j] = this[i, j];
                }
            }

            return result;
        }

        private int MultipleOf(int number, int multiple = 8) {
            if (multiple == 0)
                return number;
            else if (number == 0)
                return multiple;

            int remainder = number % multiple;

            return number + multiple - remainder;
        }

        private int GetIndex(int i, int j) {
            return MultipleOf(i * N + j) / 8 - 1;
        }

        private byte GetElement(int i, int j) {
            if (i >= M || j >= N)
                return 0xFF;

            int element = _byteArray[GetIndex(i, j)];
            int mask = 0x01;
            mask <<= (i * N + j) % 8;

            element &= mask;

            return (byte)(element >> (i * N + j) % 8);
        }

        private void SetElement(int value, int i, int j) {
            if (i >= M || j >= N || value > 1)
                throw new System.Exception(_settingElementError);

            int index = GetIndex(i, j);
            int element = _byteArray[index];
            int mask = 0x01;
            mask <<= (i * N + j) % 8;
            mask = ~mask;
            element &= mask;

            value <<= (i * N + j) % 8;
            element |= value;
            _byteArray[index] = (byte)element;
        }
    }
}
