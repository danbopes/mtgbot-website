using System.ComponentModel.DataAnnotations;

namespace MTGO.Web.Models
{
    public class TriviaModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
    }
}