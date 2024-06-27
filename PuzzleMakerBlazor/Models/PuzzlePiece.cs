using System.ComponentModel;

namespace PuzzleMakerBlazor.Models
{
    public class PuzzlePiece : INotifyPropertyChanged
    {
        public string ImagePath { get; set; } = "";
        public PieceIndex Index { get; } = new ValueTuple<int, int>(-1, -1);
        
        public Point Position { get => position; set => position = startPosition; }
        private Point position;
        private Point startPosition;

        public PuzzlePiece()
        {
            ImagePath = "";
            Index = new ValueTuple<int, int>(-1, -1);
            position = new ValueTuple<int, int>(0, 0);
            startPosition = new ValueTuple<int, int>(0, 0);
        }

        public PuzzlePiece(string path, int rIdx, int cIdx, int startX, int startY) 
        {
            ImagePath = path;
            Index = new ValueTuple<int, int>(rIdx, cIdx);
            position = new ValueTuple<int, int>(startX, startY);
            startPosition = new ValueTuple<int, int>(startX, startY);
        }

        public void SetPosition(int x, int y)
        {
            position = new ValueTuple<int, int>(x, y);
            OnPropertyChanged(nameof(position));
        }

        public void ChangePosition(int x, int y)
        {
            position = new ValueTuple<int, int>(position.x + x, position.y + y);
            OnPropertyChanged(nameof(position));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
