
using PuzzleMakerBlazor.Models;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;

namespace PuzzleMakerBlazor.Services
{
    public static class PuzzleMakerAPI
    {
        private static HttpClient httpClient;

        private static void Init()
        {
            httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<PuzzleMakerResponce> CreatePuzzle(PuzzleGenerationParameters puzzleParameters)
        {
            if (httpClient == null) 
                Init();

            using (var form = new MultipartFormDataContent())
            {
                string fileExtension = Path.GetExtension(puzzleParameters.Image!.Name);
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
                
                try
                {
                    response.EnsureSuccessStatusCode();
                    using var stream = await response.Content.ReadAsStreamAsync();
                    var (images, puzzleData) = await ProcessZip(stream);

                    return new PuzzleMakerResponce(response.StatusCode, string.Empty, puzzleData, images);
                }
                catch (HttpRequestException e)
                {
                    Dictionary<string, string> responceContent = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    responceContent.TryGetValue("errorMessage", out string message);
                    throw new HttpRequestException(message, e, e.StatusCode);
                }
            }
        }

        private static async Task<Tuple<Dictionary<string, string>, string>> ProcessZip(Stream zipStream)
        {
            using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read);
            Dictionary<string, string> imagesBase64 = new Dictionary<string, string>();
            string puzzleDataJsonRaw = "";
            foreach (var file in zip.Entries)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await file.Open().CopyToAsync(ms);
                    byte[] fileBytes = ms.ToArray();
                    if (Path.GetExtension(file.FullName) == ".json")
                        puzzleDataJsonRaw = Encoding.UTF8.GetString(fileBytes);
                    else
                        imagesBase64.Add(file.Name.Split('.')[0], Convert.ToBase64String(fileBytes));
                }
            }
            return new Tuple<Dictionary<string, string>, string>(imagesBase64, puzzleDataJsonRaw);
        }

    }
}
