import os
import glob
from typing import Final
from json import dump
from math import ceil

from PIL import Image, ImageFilter, ImageOps 

from PuzzlePiece import PuzzlePiece
from Structures import *
from Enums import *
from PuzzleMap import PuzzleMap
from Joints import *


class ImageSplitter:
    WHITE: Final[tuple[int, int, int]] = (255, 255, 255)
    BLACK: Final[tuple[int, int, int]] = (0, 0, 0)

    @classmethod
    def _makePuzzlePiece(cls, w:int, h:int, mw:int, mh:int, puzzleMap:PuzzleMap, piece:PuzzlePiece):
        def insertJoint(original:Image, jointImg:Image, x:int, y:int):
            original.paste(jointImg, (x, y))

        size = Vector2(w, h)
        margin = Vector2(mw, mh)
        mid = Vector2((size.x + margin.x) // 2, (size.y + margin.y) // 2)
        pieceSize = Vector2(size.x+margin.x, size.y+margin.y)

        im = Image.new(mode="RGBA", size=pieceSize, color=cls.WHITE)
        pixels = im.load()
        for x in range(im.size[0]):
            for y in range(im.size[1]):
                if (y < margin.y // 2 or y > size.y + margin.y // 2):
                    pixels[x, y] = cls.BLACK

                if (x < margin.x // 2 or x > size.x + margin.x // 2):
                    pixels[x, y] = cls.BLACK

        for side in piece.edges.keys():
            if (piece.edges[side].connection == Connection.NONE):
                continue

            oppositeJoint = puzzleMap.getOppositeJoint(piece, side)
            if (not (type(oppositeJoint) is JointStub)):
                jointClass = type(oppositeJoint)
            else:
                jointClass = JoinFactory.randomJointType()

            joint = jointClass.new(size.x, size.y, mw, side, piece.edges[side].connection == Connection.OUT)
            piece.edges[side] = joint
            insertJoint(im, joint.image, joint.offset.x, joint.offset.y)

        return im

    @staticmethod
    def _cut(img:Image, x, y, w, h):
        return img.crop((x, y, x + w, y + h))

    @staticmethod
    def _cutWithMargin(img:Image, x, y, w, h, m):
        return img.crop((x - m // 2, y - m // 2, x + w + m // 2, y + h + m // 2))

    @staticmethod
    def _maskImage(imagePiece:Image, margin:int, puzzleMap:PuzzleMap, puzzlePiece:PuzzlePiece):
        puzzleMask = ImageSplitter._makePuzzlePiece(imagePiece.size[0] - margin, imagePiece.size[1] - margin, margin, margin, puzzleMap, puzzlePiece).convert("L")
        imagePiece.putalpha(puzzleMask)
        return imagePiece

    @staticmethod
    def _replaceColor(img:Image, colorToReplace:tuple[int, int, int], replacer:tuple[int, int, int]):
        pixdata = img.load()
        for y in range(img.size[1]):
            for x in range(img.size[0]):
                if pixdata[x, y] == (*colorToReplace, 255):
                    pixdata[x, y] = (*replacer, 255)

    @staticmethod
    def _addBorder(img:Image, color:tuple[int, int, int], thickness:int = 1):
        if (thickness == 0):
            return img
        
        _, _, _, a = img.split()
        # find edges
        edges = a.filter(ImageFilter.FIND_EDGES)
        edges = edges.convert("RGBA")
        edgesEnchaced = edges.filter(ImageFilter.MaxFilter(thickness))
        edgesInvert = ImageOps.invert(edgesEnchaced.convert("L"))
        # color edges    
        ImageSplitter._replaceColor(edgesEnchaced, ImageSplitter.WHITE, color)
        # add edges to original image
        imgWithEdges = Image.composite(img, edgesEnchaced, edgesInvert)
        return imgWithEdges    

        
    @staticmethod
    def _clearFolder(folder:str):
        folder = folder.replace('/', os.path.sep)
        path = folder + '*.png' if folder[-1] == os.path.sep else folder + f'{os.path.sep}*.png'
        files = glob.glob(path)
        for f in files:
            os.remove(f)

    @staticmethod
    def splitImage(imgPath:str, saveFolder:str, puzzleMap:PuzzleMap, borderSize:int, safeMode:bool, scale:int=1):
        if (not safeMode):
            ImageSplitter._clearFolder(saveFolder)
    
        with Image.open(imgPath) as img:
            img = img.resize((ceil(img.size[0] * scale), ceil(img.size[1] * scale)))
            
            w, h = img.size
            pieceSize = Vector2(w / puzzleMap.columns, h / puzzleMap.rows)
            margin = ceil(min(pieceSize.x, pieceSize.y) * 0.52)
            stepX = pieceSize[0]
            stepY = pieceSize[1]
            y = 0
            for r in range(puzzleMap.rows):
                x = 0
                for c in range(puzzleMap.columns):
                    piece = ImageSplitter._cutWithMargin(img, x, y, pieceSize.x, pieceSize.y, margin)
                    piece = ImageSplitter._maskImage(piece, margin, puzzleMap, puzzleMap.getPiece(r, c))
                    piece = ImageSplitter._addBorder(piece, (75,75,75), borderSize)
                    piece.save(saveFolder + f"{r}_{c}.png")
                    x += stepX
                y += stepY

            ImageSplitter.createSplitedImageMetadata(saveFolder, Vector2(puzzleMap.rows, puzzleMap.columns), pieceSize, margin, puzzleMap)
    

    @staticmethod
    def createSplitedImageMetadata(saveFolder:str, puzzleSize:Vector2, pieceSize:Vector2, margin:int, puzzleMap:PuzzleMap):
        def calculateJointsOffset(r, c):
            halfMarging = margin / 2
            jointOffsetStepOut = Vector2(pieceSize.x / 2 + halfMarging - halfMarging / 2, pieceSize.y / 2 + halfMarging - halfMarging / 2)
            jointOffsetStepIn = Vector2(pieceSize.x / 2 - halfMarging + halfMarging / 2, pieceSize.y / 2 - halfMarging + halfMarging / 2)
            
            joints = {}
            piece = puzzleMap.getPiece(r, c)
            connected:Index
            for connected in piece.getConnectedPieceIndexes():
                direction = Index(connected.row - r, connected.column - c)
                if (piece.edges[Sides.directionToSide(direction)].connection == Connection.OUT):
                    offset = { 'x': direction.column * jointOffsetStepOut.x, 'y': direction.row * jointOffsetStepOut.y }
                else:
                    offset = { 'x': direction.column * jointOffsetStepIn.x, 'y': direction.row * jointOffsetStepIn.y }
                    
                joints[f"{connected.row}_{connected.column}"] = offset

            return { "joints": joints }


        data = {
            "puzzleSize": { 'rows': puzzleSize[0], 'columns': puzzleSize[1] },
            "pieceSize": { 'x': pieceSize[0], 'y': pieceSize[1] },
            "margin": margin,
            "pieces": {}
        }

        path = saveFolder + 'puzzleData.json'
        
        for r in range(puzzleSize[0]):
            for c in range(puzzleSize[1]):
                data['pieces'][f'{r}_{c}'] = calculateJointsOffset(r, c)

        with open(path, 'w', encoding='utf-8') as f:
            dump(data, f, ensure_ascii=False)


    
    @staticmethod
    def joinImages(imagesFolder, rows, columns, margin):
        path = imagesFolder + '*.png'
        files = glob.glob(path)
        pieceSize: tuple[int, int]
        with Image.open(files[0]) as img:
            pieceSize = img.size
        
        connectedImages = Image.new("RGBA", (pieceSize[0] * columns, pieceSize[1] * rows))

        for f in files:
            with Image.open(f) as img:
                r, c = map(int, f.split('\\')[-1].split('.')[0].split('_'))
                offsetX = 0 if (c == 0) else margin
                offsetY = 0 if (r == 0) else margin

                pastePosition = (pieceSize[0] * c - offsetX * c, pieceSize[1] * r - offsetY * r)
                connectedImages.paste(img, pastePosition, img)

        connectedImages.show()



def test():
    from Joints import JointStub
    connections = {
        Sides.TOP : JointStub.stub(),
        Sides.RIGHT : JointStub.stub(),
        Sides.BOTTOM : JointStub.stub(),
        Sides.LEFT : JointStub.stub()
    }
    margin = 60
    puzzleSize = (3, 3)
    puzzleMap = PuzzleMap(*puzzleSize)
    puzzleMap.solvePuzzle()
    file = r"D:\_Images\_Current\__lily_and_umbral_knight_ender_lilies_quietus_of_the_knights_drawn_by_yamada_ayumi_ayame__5a6f0f3a40f53df7885fc41177b90e77.jpg"
    ImageSplitter.splitImage(file, r".\\pythonServer\\generatedData\\", puzzleMap, 1, False)

    # piece = ImageSplitter._makePuzzlePiece(150, 150, margin, margin, connections)
    # piece.show()

if __name__ == "__main__":       
    test()
    # puzzleSize = (7, 7)
    # puzzleMap = PuzzleMap(*puzzleSize)
    # puzzleMap.solvePuzzle()
    # SAVE_FOLDER = "C:\\Users\\Vlad\\Desktop\\testApp\\puzzle\\src\\puzzlePieces\\"
    # margin = 35
    # file = r"D:\_Images\_Current\__lily_and_umbral_knight_ender_lilies_quietus_of_the_knights_drawn_by_yamada_ayumi_ayame__5a6f0f3a40f53df7885fc41177b90e77.jpg"
    # ImageSplitter.splitImage(file, SAVE_FOLDER, puzzleMap, 1, False)
    # ImageSplitter.joinImages(SAVE_FOLDER, *puzzleSize, margin)