using static PuzzleMakerBlazor.Models.PuzzleData;

namespace PuzzleMakerBlazor.Models
{
    public class PuzzleDataIntermediate
    {
        public Dictionary<string, int> puzzleSize { get; set; }
        public Dictionary<string, float> pieceSize { get; set; }
        public int margin { get; set; }
        public Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, float>>>> pieces { get; set; }

        private (int, int) StrIndexToTuple(string index)
        {
            string[] values = index.Split('_');
            return new ValueTuple<int, int>(int.Parse(values[0]), int.Parse(values[1]));
        }

        public PuzzleData Format()
        {
            int rows = puzzleSize["rows"];
            int columns = puzzleSize["columns"];
            PieceData[,] pieceDatas = new PieceData[columns, rows];
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    pieceDatas[x, y] = new PieceData() { index = new ValueTuple<int, int>(x, y), joints = new List<JointData>() };
                    Dictionary<string, Dictionary<string, float>> joints = pieces[string.Format("{0}_{1}", y, x)]["joints"]; // TODO: change key [r,c] order
                    foreach (KeyValuePair<string, Dictionary<string, float>> entry in joints)
                    {
                        pieceDatas[x, y].joints.Add(new JointData 
                            { 
                                connectTo = StrIndexToTuple(entry.Key),
                                jointPosition = new ValueTuple<int, int>((int)joints[entry.Key]["x"], (int)joints[entry.Key]["y"])
                            }
                        );
                    }
                }
            }

            return new PuzzleData(puzzleSize["rows"], puzzleSize["columns"], pieceSize["x"], pieceSize["y"], margin, pieceDatas);
        }
    }

    public class PuzzleData
    {
        public class JointData
        {
            public PieceIndex connectTo { get; internal set; }
            public Point jointPosition { get; internal set; }
        }

        public class PieceData
        {
            public PieceIndex index { get; internal set; }
            public List<JointData> joints { get; internal set; }
        }


        public (int Rows, int Columns) puzzleSize { get; }
        public (float X, float Y) pieceSize { get; }
        public int margin { get; }
        public PieceData[,] pieces { get; }

        public PuzzleData(int rows, int columns, float pieceWidth, float pieceHeight, int margin, PieceData[,] pieces)
        {
            puzzleSize = (rows, columns);
            pieceSize = (pieceWidth, pieceHeight);
            this.margin = margin;
            this.pieces = pieces;
        }

        public JointData GetJoint(PieceIndex from, PieceIndex to)
        {
            return pieces[from.column, from.row].joints.Where(j => j.connectTo.Equals(to)).Single();
        }

    }
}
