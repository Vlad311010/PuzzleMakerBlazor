from math import ceil
from abc import ABC, abstractmethod, ABCMeta
from random import random

from PIL import Image, ImageDraw

from Structures import Vector2
from Enums import *




class JointBase(object, metaclass=ABCMeta):
    def __init__(self, jointImage:Image, jointOffset:tuple[int, int], connection:Connection) -> None:
        self.image:Image = jointImage
        self.offset:tuple[int, int] = jointOffset
        self.connection:Connection = connection

    def _getColor(positive):
        return (255, 255, 255) if positive else (0, 0, 0)

    @classmethod
    @abstractmethod
    def new(cls, pieceWidth:int, pieceHeight:int, margin:int, side:Sides, positive:bool):
        # create class object instance with image, offset for insertion and joint generation parameters
        pass


class JointStub(JointBase):
    
    @classmethod
    def new(cls, pieceWidth:int, pieceHeight:int, margin:int, side:Sides, positive:bool):
        joint = Image.new("RGB", size=(pieceWidth + margin, pieceHeight + margin), color=(255, 255, 255))
        return cls(joint, Vector2(0, 0), Connection.UNDEFINED)

    
    @classmethod
    def stub(cls):
        joint = Image.new("RGB", size=(0, 0), color=(255, 255, 255))
        return cls(joint, Vector2(0, 0), Connection.UNDEFINED)


    
class JointRectangle(JointBase):
    @classmethod
    def new(cls, pieceWidth:int, pieceHeight:int, margin:int, side:Sides, positive:bool):
        color = cls._getColor(positive)
        jointSize = ceil(min(pieceWidth, pieceHeight) * 0.40)
        mid = Vector2((pieceWidth + margin) // 2, (pieceHeight + margin) // 2)
        negativeOffset = margin // 2

        joint: Image
        offset: tuple[int, int]
        if (side == Sides.LEFT):
            joint = Image.new("RGB", size=(margin//2, jointSize), color=color)
            offset = Vector2((not positive) * negativeOffset, mid.y - (jointSize // 2))
        elif (side == Sides.RIGHT):
            joint = Image.new("RGB", size=(margin//2, jointSize), color=color)
            offset = Vector2(margin // 2 + pieceWidth + 1 - (not positive) * negativeOffset, mid.y - (jointSize // 2))
        elif (side == Sides.TOP):
            joint = Image.new("RGB", size=(jointSize, margin//2), color=color)
            offset = Vector2(mid.x - (jointSize // 2), (not positive) * negativeOffset)
        else: # (side == Sides.BOTTOM)
            joint = Image.new("RGB", size=(jointSize, margin//2), color=color)
            offset = Vector2(mid.x - (jointSize // 2), margin // 2 + pieceHeight + 1 - (not positive) * negativeOffset)

        return cls(joint, offset, Connection.OUT if positive else Connection.IN)
    

class JointTriangle(JointBase):
    @classmethod
    def new(cls, pieceWidth:int, pieceHeight:int, margin:int, side:Sides, positive:bool):
        color = cls._getColor(positive)
        jointSize = ceil(min(pieceWidth, pieceHeight) * 0.5)
        mid = Vector2((pieceWidth + margin) // 2, (pieceHeight + margin) // 2)
        negativeOffset = margin // 2

        joint: Image
        offset: tuple[int, int]
        if (side == Sides.LEFT):
            joint = Image.new("RGB", size=(margin//2, jointSize), color=cls._getColor(not positive))
            offset = Vector2((not positive) * negativeOffset, mid.y - (jointSize // 2))
            draw = ImageDraw.Draw(joint, "RGB")
            if positive:
                points = ((margin//2, 0), (margin//2, jointSize), (0, jointSize//2))
            else:
                points = ((0, 0), (0, jointSize), (margin//2, jointSize//2))
            draw.polygon(points, fill=color)
        elif (side == Sides.RIGHT):
            joint = Image.new("RGB", size=(margin//2, jointSize), color=cls._getColor(not positive))
            offset = Vector2(margin // 2 + pieceWidth + 1 - (not positive) * negativeOffset, mid.y - (jointSize // 2))
            draw = ImageDraw.Draw(joint, "RGB")
            if positive:
                points = ((0, 0), (0, jointSize), (margin//2, jointSize//2))
            else:
                points = ((margin//2, 0), (margin//2, jointSize), (0, jointSize//2))
            draw.polygon(points, fill=color)
        elif (side == Sides.TOP):
            joint = Image.new("RGB", size=(jointSize, margin//2), color=cls._getColor(not positive))
            offset = Vector2(mid.x - (jointSize // 2), (not positive) * negativeOffset)
            draw = ImageDraw.Draw(joint, "RGB")
            if positive:
                points = ((0, margin//2), (jointSize, margin//2), (jointSize//2, 0))
            else:
                points = ((0, 0), (jointSize, 0), (jointSize//2, margin//2))
            draw.polygon(points, fill=color)
        else: # (side == Sides.BOTTOM)
            joint = Image.new("RGB", size=(jointSize, margin//2), color=cls._getColor(not positive))
            offset = Vector2(mid.x - (jointSize // 2), margin // 2 + pieceHeight + 1 - (not positive) * negativeOffset)
            draw = ImageDraw.Draw(joint, "RGB")
            if positive:
                points = ((0, 0), (jointSize, 0), (jointSize//2, margin//2))
            else:
                points = ((0, margin//2), (jointSize, margin//2), (jointSize//2, 0))
            draw.polygon(points, fill=color)

        return cls(joint, offset, Connection.OUT if positive else Connection.IN)
    

class JoinFactory():

    _defaultProbabilities = [(0.6, JointRectangle), (1, JointTriangle)]

    @staticmethod
    def randomJointType(probabilities:list[tuple[float, JointBase]] = _defaultProbabilities) -> JointBase:
        value = random()
        for p, c in probabilities:
            if (value < p):
                return c

        return probabilities[-1][1]



