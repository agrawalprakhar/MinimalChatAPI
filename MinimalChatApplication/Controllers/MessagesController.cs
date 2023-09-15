using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Data;
using MinimalChatApplication.Models;

namespace MinimalChatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly MinimalChatContext _context;

        public MessagesController(MinimalChatContext context)
        {
            _context = context;
        }

       

        

    
       
      

        [HttpPost("/api/messages")]
        public async Task<ActionResult<sendMessageResponse>> sendMessages(sendMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "message sending failed due to validation errors." });
            }
            //var senderId = GetCurrentUserId();
            var currentUser = HttpContext.User;

            var senderId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Create a new Message object based on the request data
            var message = new Message
            {
                SenderId = Convert.ToInt32(senderId),

                Content = request.Content,
                ReceiverId = request.ReceiverId,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();


            // Return a SendMessageResponse with the relevant message data
            var response = new sendMessageResponse
            {
                MessageId = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                Timestamp = message.Timestamp
            };

            return Ok(response);
        }

        [HttpPut("{messageId}")]
        public async Task<IActionResult> EditMessage(int messageId, [FromBody] EditMessage editMessage)
        {
            //var currentUser = HttpContext.User;
            var userId = GetCurrentUserId();
            // var currentUserId = Convert.ToInt32(currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (userId == -1)
            {
                return Unauthorized(new { message = "Unauthorized access" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "invalid request parameter." });
            }

            var existingMessage = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId && (m.SenderId == userId || m.ReceiverId == userId));

            //Console.WriteLine(existingMessage);

            if (existingMessage == null)
            {
                return NotFound(new { error = "Message not found." });
            }



            // Update the message content
            existingMessage.Content = editMessage.Content;
            existingMessage.Timestamp = DateTime.Now;

            // Save the changes to the database
            await _context.SaveChangesAsync();

            // Return 200 OK with a success message
            return Ok(new { message = "Message edited successfully" });

        }

        // DELETE: api/Messages/5
        [HttpDelete("/api/messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {


            var currentUser = HttpContext.User;
            var currentUserId = Convert.ToInt32(currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value);


            var message = await _context.Messages
                .Where(m => m.Id == messageId && (m.SenderId == currentUserId))
                .SingleOrDefaultAsync();

            if (message == null)
            {
                return NotFound(new { message = "Message not found" });
            }


            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message deleted successfully" });
        }


        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }

        private int GetCurrentUserId()
        {
            var currentUser = HttpContext.User;
            var currentUserId = Convert.ToInt32(currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return currentUserId;
        }
    }
}
