using CodingApp.Entities;
using System.Data;

namespace CodingApp.Auxiliary.Internal {
    public static class BindingHelper {
        public static DataView GetBindable2DArray(BinaryMatrix matrix) {
            var dataTable = new DataTable();
            int i;
            for (i = 0; i < matrix.N; i++) {
                dataTable.Columns.Add(i.ToString(), typeof(Ref<byte>));
            }

            for (i = 0; i < matrix.M; i++) {
                var dataRow = dataTable.NewRow();
                dataTable.Rows.Add(dataRow);
            }

            var dataView = new DataView(dataTable);
            for (i = 0; i < matrix.M; i++) {
                for (int j = 0; j < matrix.N; j++) {
                    int x = i, y = j;
                    var refT = new Ref<byte>(() => matrix[x, y],
                                            z => { matrix[x, y] = z; }
                                            );
                    dataView[i][j] = refT;
                }
            }

            return dataView;
        }
    }
}
