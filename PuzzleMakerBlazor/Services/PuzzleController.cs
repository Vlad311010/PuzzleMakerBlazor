using PuzzleMakerBlazor.Interfaces;
using PuzzleMakerBlazor.Models;
using System;
using System.Linq;
using static PuzzleMakerBlazor.Models.PuzzleData;

namespace PuzzleMakerBlazor.Services
{
    public class PuzzleController
    {
        private IPuzzleDataProvider puzzleData;
        private PuzzlePiece[,] puzzlePieces;
        private List<List<PieceIndex>> ConnectedPieces;

        private const int startOffsetX = 70;
        private const int startOffsetY = 110;
        private const int paddingX = 100;
        private const int paddingY = 100;

        public PuzzleController(IPuzzleDataProvider puzzleData)
        {
            SwitchDataPovider(puzzleData);
        }

        public void SwitchDataPovider(IPuzzleDataProvider puzzleData)
        {
            this.puzzleData = puzzleData;
            puzzlePieces = CreatePieces(puzzleData);
            ConnectedPieces = new List<List<PieceIndex>>();
        }

        private static PuzzlePiece[,] CreatePieces(IPuzzleDataProvider puzzleData)
        {
            PuzzlePiece[,] puzzlePieces = new PuzzlePiece[puzzleData.Columns, puzzleData.Rows];
            PieceIndex[] shuffledIndexes = GetShuffledIdexes(puzzleData.Seed, puzzleData.Columns, puzzleData.Rows);
            for (int r = 0; r < puzzleData.Rows; r++)
            {
                for (int c = 0; c < puzzleData.Columns; c++)
                {
                    PieceData pieceData = puzzleData.GetPieceData(new ValueTuple<int,int>(r, c));
                    string key = string.Format("{0}_{1}", r, c);
                    PieceIndex positionIndex = shuffledIndexes[r * puzzlePieces.GetLength(0) + c];
                    puzzlePieces[c, r] = new PuzzlePiece(puzzleData.PieceImages[key], r, c, 
                        startOffsetX + ((int)puzzleData.PieceWidth + paddingX) * positionIndex.column, 
                        startOffsetY + ((int)puzzleData.PieceHeight + paddingY) * positionIndex.row, pieceData.joints);
                }
            }

            return puzzlePieces;
        }

        private static PieceIndex[] GetShuffledIdexes(int seed, int columns, int rows)
        {
            Random random = new Random(seed);
            PieceIndex[] indexes = new PieceIndex[columns * rows];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    indexes[r * columns + c] = new ValueTuple<int, int>(r, c);
                }
            }
            return indexes.OrderBy(i => random.Next()).ToArray();
        }

        public Tuple<int, int> GetPuzzleBoardSize()
        {
            int x = startOffsetX + ((int)puzzleData.PieceWidth + paddingX) * puzzleData.Columns * 2;
            int y = startOffsetY + ((int)puzzleData.PieceHeight + paddingY) * puzzleData.Rows * 2;
            return new Tuple<int, int>(x, y);
        }

        public PuzzlePiece GetPiece(int r, int c)
        {
            return puzzlePieces[c, r];
        }

        public PuzzlePiece GetPiece(PieceIndex index)
        {
            return puzzlePieces[index.column, index.row];
        }

        public JointData GetJoint(PieceIndex from, PieceIndex to)
        {
            return GetPiece(from).Joints.Where(j => j.connectTo.Equals(to)).Single();
        }

        private List<PieceIndex> GetConnectedPieces(PieceIndex index, bool excludeSelf = false)
        {
            List<PieceIndex> connected = ConnectedPieces.Where(e => e.Contains(index)).FirstOrDefault(new List<PieceIndex>());
            return connected.Where(idx => !excludeSelf || !idx.Equals(index)).ToList();
        }

        public void SetPiecePosition(PieceIndex index, int x, int y)
        {
            PuzzlePiece piece = GetPiece(index);
            int deltaX = x - piece.Position.x;
            int deltaY = y - piece.Position.y;
            piece.SetPosition(x, y);
            MoveConnectedPieces(GetConnectedPieces(index, excludeSelf: true), deltaX, deltaY);
        }

        private void MoveConnectedPieces(List<PieceIndex> connected, int deltaX, int deltaY)
        {
            foreach (PieceIndex idx in connected)
            {
                GetPiece(idx).ChangePosition(deltaX, deltaY);
            }
        }
        public void OnPieceDown(PieceIndex pieceIdx)
        {
            foreach (var joint in GetPiece(pieceIdx).Joints)
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

        private bool CanBeConnected(PieceIndex pieceIdx, PieceIndex anotherPieceIdx)
        {
            if (AreInTheSameConnectionGroup(pieceIdx, anotherPieceIdx))
                return false;

            PuzzlePiece piece = GetPiece(pieceIdx);
            PuzzlePiece another = GetPiece(anotherPieceIdx);

            PuzzleData.JointData joint = GetJoint(pieceIdx, anotherPieceIdx);
            int jointPosX = piece.Position.x + joint.jointPosition.x;
            int jointPosY = piece.Position.y + joint.jointPosition.y;

            PuzzleData.JointData anotherJoint = GetJoint(anotherPieceIdx, pieceIdx);
            int anotherJointPosX = another.Position.x + anotherJoint.jointPosition.x;
            int anotherJointPosY = another.Position.y + anotherJoint.jointPosition.y;
            float distance = Distance(jointPosX, jointPosY, anotherJointPosX, anotherJointPosY);
            return distance < puzzleData.Margin / 2f;
        }

        public bool CanBeConnected(PieceIndex pieceIdx)
        {
            foreach (var joint in GetPiece(pieceIdx).Joints)
            {
                if (CanBeConnected(pieceIdx, joint.connectTo))
                    return true;
            }

            return false;
        }

        private float Distance(int x1, int y1, int x2, int y2)
        {
            return (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        private void Connect(PieceIndex pieceIdx1, PieceIndex pieceIdx2)
        {
            SnapPieceTo(pieceIdx1, pieceIdx2);

            // modify connectedPieces list 
            var connected1 = GetConnectedPieces(pieceIdx1);
            var connected2 = GetConnectedPieces(pieceIdx2);
            if (connected1.Count > 0 && connected2.Count > 0)
            {
                ConnectedPieces = ConnectedPieces.Where(e => !e.Contains(pieceIdx1) && !e.Contains(pieceIdx2)).ToList();
                connected1.AddRange(connected2);
                ConnectedPieces.Add(connected1);
            }
            else if (connected1.Count > 0)
            {
                ConnectedPieces.Find(e => e.Contains(pieceIdx1)).Add(pieceIdx2);
            }
            else if (connected2.Count > 0)
            {
                ConnectedPieces.Find(e => e.Contains(pieceIdx2)).Add(pieceIdx1);
            }
            else
            {
                ConnectedPieces.Add(new List<(int row, int column)> { pieceIdx1, pieceIdx2 });
            }
        }

        private void SnapPieceTo(PieceIndex movedPiece, PieceIndex snapTo)
        {
            PuzzleData.JointData joint = GetJoint(movedPiece, snapTo);
            int offsetX = -Math.Sign(joint.jointPosition.x) * (int)(puzzleData.PieceWidth) + Math.Sign(joint.jointPosition.x) * 2;
            int offsetY = -Math.Sign(joint.jointPosition.y) * (int)(puzzleData.PieceHeight) + Math.Sign(joint.jointPosition.y) * 2;

            PuzzlePiece snapToPiece = GetPiece(snapTo);
            SetPiecePosition(movedPiece, snapToPiece.Position.x + offsetX, snapToPiece.Position.y + offsetY);
            //TODO: move connected pieces
        }
    }
}
