using PuzzleMakerBlazor.Interfaces;
using PuzzleMakerBlazor.Models;
using static PuzzleMakerBlazor.Models.PuzzleData;

namespace PuzzleMakerBlazor.Services
{
    public class PuzzleDataProviderMock : IPuzzleDataProvider
    {
        Dictionary<string, string> imagesBase64 = new Dictionary<string, string>();
        PieceData pieceData = new PieceData();

        private PuzzleDataProviderMock() { }

        public static async Task<PuzzleDataProviderMock> CreatePuzzleDataProvider(PuzzleGenerationParameters generationParameters)
        {
            PuzzleDataProviderMock puzzleDataProvider = new PuzzleDataProviderMock();
            return puzzleDataProvider;
        }

        public int Rows => 0;
        public int Columns => 0;
        public float PieceWidth => 0;
        public float PieceHeight => 0;
        public int Margin => 0;
        public Dictionary<string, string> PieceImages => imagesBase64;
        public PieceData GetPieceData(PieceIndex index)
        {
            return pieceData;
        }
    }
}
