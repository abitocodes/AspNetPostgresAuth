using System.ComponentModel.DataAnnotations;

namespace AspNetPostgresAuth.Models
{
    public class RegisterViewModel{
        [Required]
        [Display(Name = "이메일")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "사용자 이름")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "비밀번호")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "비밀번호 확인")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "비밀번호가 일치하지 않습니다.")]
        public string ConfirmPassword { get; set; }
    }
}