using PuzzleMakerBlazor.Models;
using static PuzzleMakerBlazor.Models.PuzzleData;

namespace PuzzleMakerBlazor.Interfaces
{
    public interface IPuzzleDataProvider
    {
        public int Rows { get; }
        public int Columns { get; }
        public float PieceWidth { get; }
        public float PieceHeight { get; }
        public int Margin { get; }
        public Dictionary<string, string> PieceImages { get; }
        public PieceData GetPieceData(PieceIndex index);
    }
}
