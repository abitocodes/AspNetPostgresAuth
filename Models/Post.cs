using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AspNetPostgresAuth.Models
{
    public class Post
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "제목을 입력해주세요.")]
        [StringLength(200, ErrorMessage = "제목은 200자를 초과할 수 없습니다.")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "내용을 입력해주세요.")]
        public string Content { get; set; } = string.Empty;
        
        // DB에서 자동으로 생성되는 시간 필드들
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // 작성자 정보
        public string AuthorId { get; set; } = string.Empty;
        public IdentityUser? Author { get; set; }
    }
} 