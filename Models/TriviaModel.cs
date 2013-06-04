using System.ComponentModel.DataAnnotations;

namespace MTGBotWebsite.Models
{
    public class TriviaModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}