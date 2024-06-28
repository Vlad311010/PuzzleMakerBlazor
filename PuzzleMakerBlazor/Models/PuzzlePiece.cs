using System.ComponentModel;
using static PuzzleMakerBlazor.Models.PuzzleData;

namespace PuzzleMakerBlazor.Models
{
    public class PuzzlePiece : INotifyPropertyChanged
    {
        public string Image { get; set; } = "";
        public PieceIndex Index { get; } = new ValueTuple<int, int>(-1, -1);
        public List<JointData> Joints { get; }

        public Point Position { get => position; set => position = startPosition; }
        private Point position;
        private Point startPosition;

        public PuzzlePiece()
        {
            Image = "";
            Index = new ValueTuple<int, int>(-1, -1);
            position = new ValueTuple<int, int>(0, 0);
            startPosition = new ValueTuple<int, int>(0, 0);
            Joints = new List<JointData>();
        }

        public PuzzlePiece(string image, int rIdx, int cIdx, int startX, int startY, List<JointData> joints) 
        {
            Image = image;
            Index = new ValueTuple<int, int>(rIdx, cIdx);
            position = new ValueTuple<int, int>(startX, startY);
            startPosition = new ValueTuple<int, int>(startX, startY);
            Joints = joints;
            OnPropertyChanged(nameof(position));
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
