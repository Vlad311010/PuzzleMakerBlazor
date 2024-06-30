from json import load, loads
from sys import path as syspath
from os.path import join, sep
import glob
from zipfile import ZipFile
import pathlib

from flask import Flask, request, send_file
from flask_cors import CORS, cross_origin

parameters = {}
with open('./serverParameters.json') as f:
    parameters = load(f)

syspath.append(parameters['puzzle_maker_module'])
from puzzleMaker import PuzzleMaker

app = Flask(__name__)
CORS(app)


def callPuzzleMaker(image, rows, columns, scale):
    PuzzleMaker.splitImage(image, rows, columns, parameters['save_folder'], 1, scale)


@app.route('/createPuzzle', methods=['POST', 'OPTIONS'])
@cross_origin()
def createPuzzle():
    body = loads(request.form['data'])
    image = request.files['image']
    if PuzzleMaker.validateParameters(image, body['puzzleSize']['rows'], body['puzzleSize']['columns'], body['scale']):
        return {'errorMessage': 'Image size is too small or splitted in too many pieces'}, 400

    callPuzzleMaker(image, body['puzzleSize']['rows'], body['puzzleSize']['columns'], body['scale'])
    createPiecesZip()
    saveFolderAbsolutePath = relativePathToAbsolute(parameters['save_folder'])
    
    return send_file(join(saveFolderAbsolutePath, parameters['zip_file_name']), mimetype='application/zip')


def createPiecesZip():
    folderPath = parameters['save_folder'].replace('/', sep)
    folderPath = join(folderPath, '*.png')
    with ZipFile(join(parameters['save_folder'], parameters['zip_file_name']), 'w') as zipfile:
        zipfile.write(join(parameters['save_folder'], 'puzzleData.json'), arcname='puzzleData.json')
        for file in glob.glob(folderPath):
            fileName = file.split(sep)[-1]
            zipfile.write(file, arcname=fileName)
            
def relativePathToAbsolute(relativePath):
    absolutePath = join(pathlib.Path().resolve(__file__), sep.join(relativePath.split(sep)[1:]))
    return absolutePath