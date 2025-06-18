using System.ComponentModel.DataAnnotations;

namespace AspNetPostgresAuth.Models
{
    public class EditProfileViewModel
    {
        // [Display(Name = "현재 이메일")]
        // public required string CurrentEmail { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "새 이메일")]
        public required string NewEmail { get; set; }

    }
}