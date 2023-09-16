using System.ComponentModel.DataAnnotations;

namespace MinimalChatApplication.Models
{
    public class EditMessage
    {

        [Required]
        public string Content { get; set; }
    }
}
