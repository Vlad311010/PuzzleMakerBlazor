
This react application to generate jigsaw puzzles out of provided image.
Frontend written in ASP.NET Core Blazor and backend in Python. The frontend and backend communicate through a Flask endpoint.

**PuzzleMakerBlazor** is an implementation of base Blazor project which written with clean architecture and best practices.

## How to use
1. Make sure you have Python (>=3.11.3) and .NET SDK (>=8.0) installed
2. Install required dependencies `pip install -r requirements.txt`
3. To Run a backend execute following command from application .\PuzzleMakerBlazor folder
`python -m flask --app .\pythonServer\server.py run`
4. In the project directory open .\PuzzleMakerBlazor and execute `dotnet run` command or use visual studio to build and run the project.
5. Open [http://localhost:5139](http://localhost:5139) to view it in your browser.  
6. Enter all required parameters and press 'Create' button
7. Have fun

## Parameters
- Puzzle size: the number of rows and columns into which the image will be splited.
- Scale: number between 0 and 1. Rescale original image by given number.
- Image: image to create puzzle from.
- Seed: defines order in which puzzle pieces will be placed.

## Screenshoots
![Screenshoot01](./src/images/screenshoot01.png)

![Screenshoot02](./src/images/screenshoot02.png)

![Screenshoot03](./src/images/screenshoot03.png)
