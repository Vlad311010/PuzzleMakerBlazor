
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;

namespace PuzzleMakerBlazor.Services
{
    public class PuzzleMakerAPI
    {
        private readonly HttpClient httpClient;

        public PuzzleMakerAPI()
        {
            httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
        }

        public async Task<Tuple<Dictionary<string, string>, string>> CreatePuzzle(Models.PuzzleGenerationParameters puzzleParameters)
        {
            using (var form = new MultipartFormDataContent())
            {
                string fileExtension = Path.GetExtension(puzzleParameters.Image!.Name);
                // Read the file into a memory stream
                ByteArrayContent imageContent;
                using (MemoryStream ms = new MemoryStream())
                {
                    await puzzleParameters.Image.OpenReadStream(maxAllowedSize: 102400000).CopyToAsync(ms);
                    byte[] fileBytes = ms.ToArray();

                    imageContent = new ByteArrayContent(fileBytes);
                }

                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(puzzleParameters.Image.ContentType);
                form.Add(imageContent, "image", puzzleParameters.Image!.Name);

                HttpContent content = JsonContent.Create(new
                    {
                        puzzleSize = new { rows = puzzleParameters.Rows, columns = puzzleParameters.Columns },
                        scale = puzzleParameters.Scale,
                    });
                form.Add(content, "data");

                HttpResponseMessage response = await httpClient.PostAsync("/createPuzzle", form);
                response.EnsureSuccessStatusCode();
                using var stream = await response.Content.ReadAsStreamAsync();
                return await ProcessZip(stream);
            }
        }

        private async Task<Tuple<Dictionary<string, string>, string>> ProcessZip(Stream zipStream)
        {
            using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read);
            // List<string> imagesBase64 = new List<string>();
            Dictionary<string, string> imagesBase64 = new Dictionary<string, string>();
            string puzzleDataJsonRaw = "";
            foreach (var file in zip.Entries)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await file.Open().CopyToAsync(ms);
                    byte[] fileBytes = ms.ToArray();
                    if (Path.GetExtension(file.FullName) == ".json")
                    {
                        puzzleDataJsonRaw = Encoding.UTF8.GetString(fileBytes);
                    }
                    else
                    {
                        imagesBase64.Add(file.Name, Convert.ToBase64String(fileBytes));
                        // imagesBase64.Add(Convert.ToBase64String(fileBytes));
                    }
                }
            }
            return new Tuple<Dictionary<string, string>, string>(imagesBase64, puzzleDataJsonRaw);
        }

    }
}
