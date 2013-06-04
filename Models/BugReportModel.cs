using System.ComponentModel.DataAnnotations;

namespace MTGBotWebsite.Models
{
    public class BugReportModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Display(Name = "What's the Problem?")]
        public string Problem { get; set; }

        [Required]
        [Display(Name = "How can I reproduce it?")]
        public string Reproduce { get; set; }
    }
}