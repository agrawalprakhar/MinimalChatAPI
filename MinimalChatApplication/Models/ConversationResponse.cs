namespace MinimalChatApplication.Models
{
    public class ConversationResponse
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public class ConversationHistoryResponseDto
        {
            public IEnumerable<ConversationResponse> Messages { get; set; }
        }
    }
}
