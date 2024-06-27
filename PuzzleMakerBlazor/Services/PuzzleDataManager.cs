using PuzzleMakerBlazor.Models;
using PuzzleMakerBlazor.Interfaces;
namespace PuzzleMakerBlazor.Services
{
    public class PuzzleDataManager
    {
        public IPuzzleDataProvider DataProvider { get; private set; }
        public event Action onDataProviderChange;

        public PuzzleDataManager() 
        {
            SwitchPuzzleDataProvider(new PuzzleGenerationParameters(), PuzzleDataProviderMock.CreatePuzzleDataProvider);
        }


        public async Task SwitchPuzzleDataProvider<T>(PuzzleGenerationParameters generationParameters, Func<PuzzleGenerationParameters, Task<T>> del) where T : IPuzzleDataProvider
        {
            DataProvider = await del(generationParameters);
            DataProviderChanged();
        }

        protected virtual void DataProviderChanged()
        {
            onDataProviderChange?.Invoke();
        }

    }
}
