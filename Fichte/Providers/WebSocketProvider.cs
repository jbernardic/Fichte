using Fichte.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;

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

        public static async Task ReceiveMessages(WebSocket webSocket, string userId)
        {
            _webSockets.TryAdd(userId.ToString(), webSocket);

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _webSockets.TryRemove(userId, out _);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                }
            }
        }
    }
}