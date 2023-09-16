using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Data;
using MinimalChatApplication.Models;
using static MinimalChatApplication.Models.ConversationResponse;

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


        [HttpGet("/api/messages")]
       
        public IActionResult GetConversationHistory([FromQuery] ConversationRequest request)
        {
            // Get the authenticated user's ID from the JWT token

            var userId = GetCurrentUserId();
            var messages = _context.Messages
                .Where(m => (m.SenderId == userId || m.ReceiverId == userId) &&
                            (!request.Before.HasValue || m.Timestamp < request.Before))
                .OrderBy(m => request.Sort == "asc" ? m.Timestamp : (DateTime?)null)
                .Take(request.Count)
                .Select(m => new
                {
                    id = m.Id,
                    senderId = m.SenderId,
                    receiverId = m.ReceiverId,
                    content = m.Content,
                    timestamp = m.Timestamp
                })
                .ToList();

            return Ok(new { messages });
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

        [HttpPut("/api/messages/{messageId}")]
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
                // Check if the message exists but was sent by another user
                var messageExists = await _context.Messages.AnyAsync(m => m.Id == messageId);
                if (messageExists)
                {
                    return Unauthorized(new { message = "You are not allowed to edit messages sent by other users" });
                }
                return NotFound(new { error = "Message not found." });
            }
            // Validate the request content
            if (string.IsNullOrWhiteSpace(editMessage.Content))
            {
                return BadRequest(new { error = "Message content is required" });
            }


            // Update the message content
            existingMessage.Content = editMessage.Content;
            existingMessage.Timestamp = DateTime.Now; 

            // Save the changes to the database
            await _context.SaveChangesAsync();

            // Return 200 OK with a success message
            return Ok(new
            {
                success = true,
                message = "Message edited successfully",
                editedMessage = new
                {
                    messageId = existingMessage.Id,
                    senderId = existingMessage.SenderId,
                    receiverId = existingMessage.ReceiverId,
                    content = existingMessage.Content,
                    timestamp = existingMessage.Timestamp
                }
            });

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
                // Check if the message exists but was sent by another user
                var messageExists = await _context.Messages.AnyAsync(m => m.Id == messageId);
                if (messageExists)
                {
                    return Unauthorized(new { message = "You are not allowed to Delete messages sent by other users" });
                }
                return NotFound(new { message = "Message not found" });
            }


            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message deleted successfully" });
        }

        private int GetCurrentUserId()
        {
            var currentUser = HttpContext.User;
            var currentUserId = Convert.ToInt32(currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return currentUserId;
        }
    }
}
