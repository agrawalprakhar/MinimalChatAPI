using System.ComponentModel.DataAnnotations;

namespace MinimalChatApplication.Models
{
    public class sendMessageRequest
    {
        [Key]
        [Required]
        public int ReceiverId { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
