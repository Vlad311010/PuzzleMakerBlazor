using PuzzleMakerBlazor.Models;
using Newtonsoft.Json;

namespace PuzzleMakerBlazor.Services
{
    public class PuzzleManager
    {
        public int Rows => puzzleData.puzzleSize.Rows;
        public int Columns => puzzleData.puzzleSize.Columns;
        public float PieceWidth => puzzleData.pieceSize.X;
        public float PieceHeight => puzzleData.pieceSize.Y;
        public int Margin => puzzleData.margin;
        public Dictionary<string, string> PieceImages => imagesBase64;


        public PuzzlePiece[,] Pieces => pieces ?? GetPieces();

        public List<List<PieceIndex>> connectedPieces = new List<List<PieceIndex>>();
        
        private PuzzleData puzzleData;
        private Dictionary<string, string> imagesBase64;

        // private PuzzleData PuzzleData => puzzleData;
        private PuzzlePiece[,]? pieces = null;

        readonly PuzzleMakerAPI puzzleMaker;
        

        private PuzzleManager() { }

        public static async Task<PuzzleManager> BuildPuzzleManagerAsync(PuzzleGenerationParameters generationParameters, PuzzleMakerAPI puzzleMakerAPI)
        {
            PuzzleManager puzzleManager = new PuzzleManager();
            (puzzleManager.imagesBase64, puzzleManager.puzzleData) = await PuzzleManager.GetPuzzleData(generationParameters, puzzleMakerAPI);
            return puzzleManager;
        }


        public PuzzlePiece[,] GetPieces()
        {
            const int stepX = 200;
            const int stepY = 170;
            PuzzlePiece[,] puzzlePieces = new PuzzlePiece[puzzleData.puzzleSize.Columns, puzzleData.puzzleSize.Rows];
            for (int c = 0; c < puzzleData.puzzleSize.Columns; c++) 
            {
                for (int r = 0; r < puzzleData.puzzleSize.Rows; r++)
                {
                    PuzzleData.PieceData pieceData = puzzleData.pieces[c, r];
                    // puzzlePieces[c, r] = new PuzzlePiece(string.Format(""".\tmp\{0}_{1}.png""", r, c), r, c, stepX * c, stepY * r);
                    puzzlePieces[c, r] = new PuzzlePiece(string.Format(""".\tmp\{0}_{1}.png""", r, c), r, c, stepX * c, stepY * r);

                }
            }

            pieces = puzzlePieces;
            return puzzlePieces;
        }

        public PuzzlePiece GetPiece(PieceIndex index) 
        {
            return Pieces[index.column, index.row];
        }


        public List<PieceIndex> GetConnectedPieces(PieceIndex index, bool excludeSelf=false)
        {
            List<PieceIndex> connected = connectedPieces.Where(e => e.Contains(index)).FirstOrDefault(new List<PieceIndex>());
            return connected.Where(idx => !excludeSelf || !idx.Equals(index)).ToList();
        }

        public void SetPiecePosition(PieceIndex index, int x, int y)
        {
            int deltaX = x - Pieces[index.column, index.row].Position.x;
            int deltaY = y - Pieces[index.column, index.row].Position.y;
            Pieces[index.column, index.row].SetPosition(x, y);
            MoveConnectedPieces(GetConnectedPieces(index, excludeSelf:true), deltaX, deltaY);
        }

        private void MoveConnectedPieces(List<PieceIndex> connected, int deltaX, int deltaY)
        {
            foreach (PieceIndex idx in connected)
            {
                Pieces[idx.column, idx.row].ChangePosition(deltaX, deltaY);
            }
        }



        private static async Task<Tuple<Dictionary<string, string>, PuzzleData>> GetPuzzleData(PuzzleGenerationParameters generationParameters, PuzzleMakerAPI puzzleMaker)
        {
            var (pieceImagesBase64, puzzleDataRaw) = await puzzleMaker.CreatePuzzle(generationParameters);
            PuzzleDataIntermediate? deserializedData = JsonConvert.DeserializeObject<PuzzleDataIntermediate>(puzzleDataRaw);
            if (deserializedData == null)
                throw new InvalidOperationException("Given json file can't be mapped to PuzzleDataIntermediate object");

            
            return new Tuple<Dictionary<string, string>, PuzzleData>(pieceImagesBase64, deserializedData.Format());
            /*PuzzleDataIntermediate? deserializedData = JsonConvert.DeserializeObject<PuzzleDataIntermediate>(File.ReadAllText("wwwroot/tmp/puzzleData.json"));
            if (deserializedData == null)
                throw new InvalidOperationException("Given json file can't be mapped to PuzzleDataIntermediate object");

            return deserializedData.Format();*/
        }

        public void OnPieceDown(PieceIndex pieceIdx)
        {
            foreach (var joint in puzzleData.pieces[pieceIdx.column, pieceIdx.row].joints)
            {
                if (CanBeConnected(pieceIdx, joint.connectTo))
                {
                    Connect(pieceIdx, joint.connectTo);
                    break;
                }
            }
        }


        private bool AreInTheSameConnectionGroup(PieceIndex pieceIdx1, PieceIndex pieceIdx2)
        {
            return GetConnectedPieces(pieceIdx1).Contains(pieceIdx2);
        }

        public bool CanBeConnected(PieceIndex pieceIdx, PieceIndex anotherPieceIdx)
        {
            if (AreInTheSameConnectionGroup(pieceIdx, anotherPieceIdx))
                return false;

            PuzzlePiece piece = GetPiece(pieceIdx);
            PuzzlePiece another = GetPiece(anotherPieceIdx);

            PuzzleData.JointData joint = puzzleData.GetJoint(pieceIdx, anotherPieceIdx);
            int jointPosX = piece.Position.x + joint.jointPosition.x;
            int jointPosY = piece.Position.y + joint.jointPosition.y;

            PuzzleData.JointData anotherJoint = puzzleData.GetJoint(anotherPieceIdx, pieceIdx);
            int anotherJointPosX = another.Position.x + anotherJoint.jointPosition.x;
            int anotherJointPosY = another.Position.y + anotherJoint.jointPosition.y;
            float distance = Distance(jointPosX, jointPosY, anotherJointPosX, anotherJointPosY);
            return distance < puzzleData.margin / 2f;
        }

        public bool CanBeConnected(PieceIndex pieceIdx)
        {
            foreach (var joint in puzzleData.pieces[pieceIdx.column, pieceIdx.row].joints)
            {
                if (CanBeConnected(pieceIdx, joint.connectTo))
                    return true;
            }

            return false;
        }

        private float Distance(int x1, int y1, int x2, int y2)
        {
            return (float)Math.Sqrt( (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1) );
        }

        public void Connect(PieceIndex pieceIdx1, PieceIndex pieceIdx2)
        {
            SnapPieceTo(pieceIdx1, pieceIdx2);

            // modify connectedPieces list 
            var connected1 = GetConnectedPieces(pieceIdx1);
            var connected2 = GetConnectedPieces(pieceIdx2);
            if (connected1.Count > 0 && connected2.Count > 0)
            {
                connectedPieces = connectedPieces.Where(e => !e.Contains(pieceIdx1) && !e.Contains(pieceIdx2)).ToList();
                connected1.AddRange(connected2);
                connectedPieces.Add(connected1);
            }
            else if (connected1.Count > 0)
            {
                connectedPieces.Find(e => e.Contains(pieceIdx1)).Add(pieceIdx2);
            }
            else if (connected2.Count > 0)
            {
                connectedPieces.Find(e => e.Contains(pieceIdx2)).Add(pieceIdx1);
            }
            else
            {
                connectedPieces.Add(new List<(int row, int column)> { pieceIdx1, pieceIdx2 });
            }
        }

        private void SnapPieceTo(PieceIndex movedPiece, PieceIndex snapTo) 
        {
            PuzzleData.JointData joint = puzzleData.GetJoint(movedPiece, snapTo);
            int offsetX = -Math.Sign(joint.jointPosition.x) * (int)(puzzleData.pieceSize.X) + Math.Sign(joint.jointPosition.x) * 2;
            int offsetY = -Math.Sign(joint.jointPosition.y) * (int)(puzzleData.pieceSize.Y) + Math.Sign(joint.jointPosition.y) * 2;

            PuzzlePiece snapToPiece = GetPiece(snapTo);
            SetPiecePosition(movedPiece, snapToPiece.Position.x + offsetX, snapToPiece.Position.y + offsetY);
            //TODO: move connected pieces
        }
    }
}
