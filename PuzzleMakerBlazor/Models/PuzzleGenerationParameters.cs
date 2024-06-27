using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace PuzzleMakerBlazor.Models
{
    public class PuzzleGenerationParameters
    {
        [Required] public int? Rows { get; set; }
        [Required] public int? Columns { get; set; }
        public float Scale { get; set; } = 0.1f;
        [Required] public IBrowserFile? Image { get; set; }
    }
}
