using ChatServer.Data;
using ChatServer.DTO;
using ChatServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

namespace ChatServer.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        private readonly ChatServerContext _context;

        private readonly IConfiguration _configuration;

        private readonly ILogger<WebSocketController> _logger;

        public static readonly Dictionary<int, List<WebSocket>> connections = new();

        private static int _numConnections = 0;

        public WebSocketController(ChatServerContext context, IConfiguration configuration, ILogger<WebSocketController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [Route("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                await EchoTest(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task EchoTest(WebSocket webSocket)
        {
            int temp = _numConnections;
            _numConnections++;
            try
            {
                _logger.LogCritical("Web socket client connected id = " + temp);
                var buffer = new byte[4096];

                _logger.LogCritical("Waiting message id = " + temp);
                WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);
                _logger.LogCritical("Web socket client send message id = "+temp);

                string receiveWithoutNull = System.Text.Encoding.UTF8.GetString(buffer);

                receiveWithoutNull = receiveWithoutNull.Replace("\0", string.Empty);

                WebSocketMessageDTO webSocketMessage = JsonSerializer.Deserialize<WebSocketMessageDTO>(receiveWithoutNull);

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

                JwtSecurityToken token = new JwtSecurityToken(webSocketMessage.data);

                string? secretKey = _configuration.GetSection("JWTSettings:SecretKey").Value;
                var issuer = _configuration.GetSection("JWTSettings:Issuer").Value;
                var audience = _configuration.GetSection("JWTSettings:Audience").Value;
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                try
                {
                    tokenHandler.ValidateToken(webSocketMessage.data, new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        IssuerSigningKey = key,
                        ValidateIssuerSigningKey = true
                    }, out SecurityToken validatedToken);
                }
                catch (Exception ex)
                {

                }

                int id_user = Convert.ToInt32(token.Claims.FirstOrDefault(t => t.Type == "Id").Value);

                _logger.LogCritical("Message is token id = " + temp);
                if (!connections.ContainsKey(id_user))
                {
                    connections.Add(id_user, new());
                    connections[id_user].Add(webSocket);
                }
                else
                {
                    connections[id_user].Add(webSocket);
                }

                buffer = new byte[4096];
                while (webSocket.State == WebSocketState.Open)
                {
                    _logger.LogCritical("Waiting message id = " + temp);
                    buffer = new byte[4096];
                    receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                _logger.LogCritical("Message is cloising id = " + temp);
                connections[id_user].Remove(webSocket);
                if (connections[id_user].Count == 0)
                {
                    connections.Remove(id_user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Message is cloising id = " + temp);
            }
            finally
            {
                _logger.LogCritical("Connection closed id = " + temp);
                await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                webSocket.Dispose();
            }
        }
    }
}
