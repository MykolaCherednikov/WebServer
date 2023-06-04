using ChatServer.Data;
using ChatServer.DTO;
using ChatServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;

namespace ChatServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ChatServerContext _context;

        public MessagesController(ChatServerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns all messages by chosen chat.
        /// </summary>
        /// <response code="400">
        /// Invalid auth_token:
        /// 
        ///     {
        ///         "code": 9007,
        ///         "message": "You are not authorized"
        ///     }
        /// 
        /// Chat does not exist:
        /// 
        ///     {
        ///         "code": 1035,
        ///         "message": "Unable to retrieve Chat. Chat with the specified primary key does not exist"
        ///     }
        ///     
        /// User has no permission to this Chat:
        /// 
        ///     {
        ///         "code": 1013,
        ///         "message": "User has no permission to this Chat"
        ///     }
        /// </response>
        [Authorize]
        [HttpGet("GetAllMessagesFromChat")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MessageDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetAllMessagesFromChat(int id_chat)
        {
            int id_user = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);

            var user = await _context.User.FirstOrDefaultAsync(x => x.id_user == id_user);
            if (user == null)
            {
                return BadRequest(new ErrorDTO(9007, "You are not authorized"));
            }

            if (!await IsChatExists(id_chat))
            {
                return BadRequest(new ErrorDTO(1035, "Unable to retrieve Chat. Chat with the specified primary key does not exist"));
            }

            if (!IsUserInChat(id_user, id_chat))
            {
                return BadRequest(new ErrorDTO(1013, "User has no permission to this Chat"));
            }

            var messages = await _context.Message.Where(m => m.rk_chat == id_chat).ToListAsync();

            List<MessageDTO> messagesDTO = new List<MessageDTO>();

            foreach (var item in messages)
            {
                var messageDTO = new MessageDTO();
                messageDTO.id_message = item.id_message;
                messageDTO.text_message = item.text_message;
                messageDTO.rk_user = item.rk_user;
                messageDTO.rk_chat = item.rk_chat;
                messageDTO.data_time = item.data_time;

                messagesDTO.Add(messageDTO);
            }

            return messagesDTO;
        }


        /// <summary>
        /// Send Message.
        /// </summary>
        /// <response code="400">
        /// Invalid auth_token:
        /// 
        ///     {
        ///         "code": 9007,
        ///         "message": "You are not authorized"
        ///     }
        /// 
        /// Chat does not exist:
        /// 
        ///     {
        ///         "code": 1035,
        ///         "message": "Unable to retrieve Chat. Chat with the specified primary key does not exist"
        ///     }
        ///     
        /// User has no permission to this Chat:
        /// 
        ///     {
        ///         "code": 1013,
        ///         "message": "User has no permission to this Chat"
        ///     }
        /// </response>
        [Authorize]
        [HttpPost("SendMessage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
        public async Task<ActionResult<MessageDTO>> SendMessage(InputMessageDTO m)
        {
            int id_user = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);

            var user = await _context.User.FirstOrDefaultAsync(x => x.id_user == id_user);
            if (user == null)
            {
                return BadRequest(new ErrorDTO(9007, "You are not authorized."));
            }

            if (!await IsChatExists(m.id_chat))
            {
                return BadRequest(new ErrorDTO(1035, "Unable to retrieve Chat. Chat with the specified primary key does not exist."));
            }

            if (!IsUserInChat(id_user, m.id_chat))
            {
                return BadRequest(new ErrorDTO(1013, "User has no permission to this Chat."));
            }

            Message message = new Message();

            message.text_message = m.text_message;
            message.rk_chat = m.id_chat;
            message.rk_user = id_user;
            message.data_time = DateTime.Now;

            var result = new MessageDTO();
            result.text_message = message.text_message;
            result.rk_user = message.rk_user;
            result.rk_chat = message.rk_chat;
            result.data_time = message.data_time;

            Message temp_message = (await _context.Message.AddAsync(message)).Entity;

            await _context.SaveChangesAsync();

            result.id_message = temp_message.id_message;

            var users_receiver_list = await _context.UserToChat.Where(c => c.rk_id_chat == m.id_chat).Select(c => c.rk_id_user).ToListAsync();

            JObject messageDTO = new JObject();

            messageDTO.Add("type", WebSocketDataType.SendMessage.ToString());
            messageDTO.Add("data", JObject.FromObject(result));

            var message_json = JsonConvert.SerializeObject(messageDTO);

            foreach(var id_user_receiver in users_receiver_list)
            {
                /*if (!WebSocketController.connections.Keys.Contains(id_user_receiver) || id_user == id_user_receiver)
                    continue;*/

                if (!WebSocketController.connections.Keys.Contains(id_user_receiver))
                    continue;

                foreach (var webSocket in WebSocketController.connections[id_user_receiver])
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.SendAsync(
                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(message_json)),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                    }
                }
            }



            return Ok(result);
        }


        [HttpGet("GetAllMessages")]
        public async Task<List<Message>> GetAllMessages()
        {
            return await _context.Message.ToListAsync();
        }

        [HttpGet("GetMessageById")]
        public async Task<Message> GetAllChats(int id)
        {
            var result = await _context.Message.FirstOrDefaultAsync(c => c.id_message == id);
            return result!;
        }

        private async Task<bool> IsChatExists(int id_chat)
        {
            bool result = await _context.Chat.FirstOrDefaultAsync(c => c.id_chat == id_chat) != null;
            return result;
        }

        private bool IsUserInChat(int id_user, int id_chat)
        {
            var usersInChat = _context.UserToChat.Where(c => c.rk_id_chat == id_chat).Select(c => c.rk_id_user).ToList();
            bool isUserInChat = usersInChat.Contains(id_user);

            return isUserInChat;
        }

        
    }
}
