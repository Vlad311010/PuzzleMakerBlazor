from __future__ import annotations
from enum import Enum
from random import random
from collections import namedtuple

Index = namedtuple('PuzzleIndex', ['row', 'column'])


class Sides(Enum):
    TOP = 1
    RIGHT = 2
    BOTTOM = 3  
    LEFT = 4

    def sideToDirection(side: Sides) -> Index:
        if (side == Sides.TOP):
            return Index(-1, 0)
        elif (side == Sides.RIGHT):
            return Index(0, 1)
        elif (side == Sides.BOTTOM):
            return Index(1, 0)
        elif (side == Sides.LEFT):
            return Index(0, -1)
        
    def directionToSide(direction: Index) -> Sides:
        if ( abs(direction.row) > 1 or abs(direction.column) > 1):
            raise ValueError("Wrong direction values")
        
        if (direction == Index(-1, 0)):
            return Sides.TOP
        elif (direction ==  Index(0, 1)):
            return Sides.RIGHT
        elif (direction == Index(1, 0)):
            return Sides.BOTTOM
        elif (direction == Index(0, -1)):
            return Sides.LEFT
        
    def oppositeSide(side: Sides) -> Sides:
        if (side == Sides.TOP):
            return Sides.BOTTOM
        elif (side == Sides.RIGHT):
            return Sides.LEFT
        elif (side == Sides.BOTTOM):
            return Sides.TOP
        elif (side == Sides.LEFT):
            return Sides.RIGHT

class Connection(Enum):
    UNDEFINED = -1
    NONE = 0
    OUT = 1
    IN = 2

    def randomConnection():
        return Connection.OUT if (random() > 0.5) else Connection.IN
    
    def oppositeConnection(joint: Connection):
        if (joint == Connection.UNDEFINED or joint == Connection.NONE):
            return joint
        elif (joint == Connection.IN):
            return Connection.OUT
        elif (joint == Connection.OUT):
            return Connection.IN

    def toString(joint):
        if (joint == Connection.NONE):
            return '-'
        elif (joint == Connection.UNDEFINED):
            return 'x'
        elif (joint == Connection.IN):
            return 'i'
        elif (joint == Connection.OUT):
            return 'o'