from Structures import *
from Enums import *
from Joints import JointStub

class PuzzlePiece:
    def __init__(self, row, column, edgesData=None) -> None:
        self.index = Index(row, column)

        if (not edgesData):
            self.edges = {
                Sides.TOP : JointStub.stub(), 
                Sides.RIGHT : JointStub.stub(), 
                Sides.BOTTOM : JointStub.stub(), 
                Sides.LEFT : JointStub.stub()
            }
        else:
            self.edges = edgesData

    def getConnectedPieceIndexes(self):
        connected = []
        for s, j in self.edges.items():
            if (j.connection == Connection.IN or j.connection == Connection.OUT):
                direction = Sides.sideToDirection(s)
                idx = Index(self.index.row + direction.row, self.index.column + direction.column)
                connected.append(idx)
        
        return connected
                

    def isDefined(self):
        return not (Connection.UNDEFINED in [j.connection for j in (Connection.UNDEFINED in self.edges.values)])
    
    def __str__(self) -> str:
        top = Connection.toString(self.edges[Sides.TOP].connection)
        right = Connection.toString(self.edges[Sides.RIGHT].connection)
        bottom = Connection.toString(self.edges[Sides.BOTTOM].connection)
        left = Connection.toString(self.edges[Sides.LEFT].connection)
        return f" {top} \n{left} {right}\n {bottom} "

