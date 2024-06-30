from Structures import *
from Enums import *
from PuzzlePiece import PuzzlePiece
from Joints import JointBase
  

class PuzzleMap:
    def __init__(self, rows:int, columns:int) -> None:
        self.rows = rows
        self.columns = columns
        self.puzzleMap: PuzzleMap = [ [PuzzlePiece(r, c) for c in range(columns)]  for r in range(rows) ]
        

    def solvePuzzle(self): # TODO: add seed parameter
        for r in range(self.rows):
            for c in range(self.columns):
                self._solvePiece(r, c)

    
    def _isValidIndex(self, row, column):
        return not (column < 0 or column >= self.columns or row < 0 or row >= self.rows)
    
    def getPiece(self, r, c) -> PuzzlePiece:
        if (not self._isValidIndex(r, c)):
            raise ValueError(f"Wrong index {r},{c}")
        
        return self.puzzleMap[r][c]
    
    def getOppositeJoint(self, piece:PuzzlePiece, side:Sides) -> JointBase:
        direction: Index = Sides.sideToDirection(side)
        adjacentPiece: PuzzlePiece
        adjacentPiece = self.getPiece(piece.index.row + direction.row, piece.index.column + direction.column)
        return adjacentPiece.edges[Sides.oppositeSide(side)]
    
    def tryGetPiece(self, r, c) -> tuple[bool, PuzzlePiece | None]:
        try:
            return (True, self.getPiece(r, c))
        except ValueError:
            return (False, None)
        
    def getPieceMap(self, r, c) -> dict[Sides, JointBase]:
        piece = self.getPiece(r, c)
        return piece.edges
        
    def _solvePiece(self, r:int, c:int):
        piece: PuzzlePiece = self.getPiece(r,c)

        # if border piece set appropriate edge connection to none
        if (r == 0):
            piece.edges[Sides.TOP].connection = Connection.NONE
        elif (r == self.rows - 1):
            piece.edges[Sides.BOTTOM].connection = Connection.NONE

        if (c == 0):
            piece.edges[Sides.LEFT].connection = Connection.NONE
        elif (c == self.columns - 1):
            piece.edges[Sides.RIGHT].connection = Connection.NONE

        # for every undefined edge set random connection (propagate to adjacent piece)
        for key in piece.edges.keys():
            if piece.edges[key].connection == Connection.UNDEFINED:
                joint = Connection.randomConnection()
                piece.edges[key].connection = joint
                direction: Index = Sides.sideToDirection(key)
                success: bool
                adjacentPiece: PuzzlePiece
                success, adjacentPiece = self.tryGetPiece(r + direction.row, c + direction.column)
                if (success):
                    adjacentPiece.edges[Sides.oppositeSide(key)].connection = Connection.oppositeConnection(joint)



    def __str__(self) -> str:
        def flatten(xss):
            return [x for xs in xss for x in xs]

        def rowToString(rowData):
            rowStrings: list[str] = list(map(lambda p: str(p).split("\n"), rowData))
            formated = list(zip(*rowStrings))
            formated = (zip(map(lambda x: ''.join(x), formated)))
            return ('\n'.join(flatten(formated)))
    
        reduced = list(map(lambda row: rowToString(row), self.puzzleMap))
        formatedData = '\n'.join(reduced)
        return formatedData
    

