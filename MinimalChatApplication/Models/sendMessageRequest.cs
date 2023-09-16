using System.ComponentModel.DataAnnotations;

namespace MinimalChatApplication.Models
{
    public class sendMessageRequest
    {
        [Key]
        public int ReceiverId { get; set; }
    
        public string Content { get; set; }
    }
}
