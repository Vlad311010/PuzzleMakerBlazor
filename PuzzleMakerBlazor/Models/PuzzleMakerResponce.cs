using Newtonsoft.Json;
using System.Net;

namespace PuzzleMakerBlazor.Models
{
    public class PuzzleMakerResponce
    {
        public HttpStatusCode StatusCode { get; }
        public string ErrorMessage { get; }
        public PuzzleData PuzzleData { get; }
        public Dictionary<string, string> Images { get; }

        public PuzzleMakerResponce(HttpStatusCode code, string message, string puzzleDataRaw, Dictionary<string, string> images)
        {
            StatusCode = code;
            ErrorMessage = message;
            Images = images;

            PuzzleDataIntermediate? deserializedData = JsonConvert.DeserializeObject<PuzzleDataIntermediate>(puzzleDataRaw);
            if (deserializedData == null)
                throw new InvalidOperationException("Given json file can't be mapped to PuzzleDataIntermediate object");
            
            PuzzleData = deserializedData.Format();
        }
    }
}
