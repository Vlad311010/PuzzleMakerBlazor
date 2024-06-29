using Newtonsoft.Json;
using PuzzleMakerBlazor.Interfaces;
using PuzzleMakerBlazor.Models;
using static PuzzleMakerBlazor.Models.PuzzleData;

namespace PuzzleMakerBlazor.Services
{
    public class PuzzleDataProviderAPI : IPuzzleDataProvider
    {
        private Dictionary<string, string> imagesBase64;
        private PuzzleData puzzleData;
        private int seed = 0;

        private PuzzleDataProviderAPI() { }

        public static async Task<PuzzleDataProviderAPI> CreatePuzzleDataProvider(PuzzleGenerationParameters generationParameters)
        {
            PuzzleMakerResponce responce = await GetPuzzleData(generationParameters);
            PuzzleDataProviderAPI puzzleDataProvider = new PuzzleDataProviderAPI();
            puzzleDataProvider.imagesBase64 = responce.Images;
            puzzleDataProvider.puzzleData = responce.PuzzleData;
            puzzleDataProvider.seed = generationParameters.Seed;
            return puzzleDataProvider;
        }

        private static async Task<PuzzleMakerResponce> GetPuzzleData(PuzzleGenerationParameters generationParameters)
        {
            return await PuzzleMakerAPI.CreatePuzzle(generationParameters);
        }

        public int Rows => puzzleData.puzzleSize.Rows;
        public int Columns => puzzleData.puzzleSize.Columns;
        public float PieceWidth => puzzleData.pieceSize.X;
        public float PieceHeight => puzzleData.pieceSize.Y;
        public int Margin => puzzleData.margin;
        public int Seed => seed;
        public Dictionary<string, string> PieceImages => imagesBase64;
        public PieceData GetPieceData(PieceIndex index)
        {
            return puzzleData.pieces[index.column, index.row];
        }

    }
}
