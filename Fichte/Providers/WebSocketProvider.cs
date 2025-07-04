using Fichte.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Fichte.Providers
{
    public class WebSocketProvider
    {
        private static readonly ConcurrentDictionary<string, WebSocket> _webSockets = new ConcurrentDictionary<string, WebSocket>();
        public static void BroadcastMessage(Message message)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(message, settings);

            foreach (var webSocket in _webSockets.Values)
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    var buffer = Encoding.UTF8.GetBytes(messageJson);
                    webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public static void BroadcastUserStatusUpdate(int userId, bool isOnline)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var statusUpdate = new { type = "userStatusUpdate", userId, isOnline };
            var statusJson = Newtonsoft.Json.JsonConvert.SerializeObject(statusUpdate, settings);

            foreach (var webSocket in _webSockets.Values)
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    var buffer = Encoding.UTF8.GetBytes(statusJson);
                    webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public static async Task ReceiveMessages(DatabaseContext context, WebSocket webSocket, string userId)
        {
            _webSockets.TryAdd(userId.ToString(), webSocket);

            var user = await context.Users.FirstOrDefaultAsync(u => u.IDUser == int.Parse(userId));
            if (user != null)
            {
                user.IsOnline = true;
                await context.SaveChangesAsync();
                BroadcastUserStatusUpdate(user.IDUser, true);
            }

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
            }
            catch (WebSocketException) {}
            finally
            {
                _webSockets.TryRemove(userId, out _);
                
                if (user != null)
                {
                    user.IsOnline = false;
                    user.LastSeen = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                    BroadcastUserStatusUpdate(user.IDUser, false);
                }

                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                }
            }
        }
    }
}