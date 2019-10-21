﻿using Microsoft.AspNetCore.Http;
using RawCMS.Plugins.ApiGateway.Classes.Settings;
using RawCMS.Plugins.ApiGateway.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RawCMS.Plugins.ApiGateway.Classes.Handles
{
    public class SocketRawHandler : RawHandler
    {
        private static readonly string[] NotForwardedWebSocketHeaders = new[] { "Connection", "Host", "Upgrade", "Sec-WebSocket-Key", "Sec-WebSocket-Version" };

        public  override HandlerProtocolType HandlerRequestType => HandlerProtocolType.Socket;

        public override async Task HandleRequest(HttpContext context, Node node, int? bufferSize = null, bool chunked = false, TimeSpan? keepAlive = null)
        {

            using (var client = new ClientWebSocket())
            {
                foreach (var headerEntry in context.Request.Headers)
                {
                    if (!NotForwardedWebSocketHeaders.Contains(headerEntry.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        client.Options.SetRequestHeader(headerEntry.Key, headerEntry.Value);
                    }
                }

                var wsScheme = string.Equals(node.Scheme, "https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws";
                string url = GetUri(context, node.Host, node.Port, node.Scheme);

                if (keepAlive.HasValue)
                {
                    client.Options.KeepAliveInterval = keepAlive.Value;
                }

                try
                {
                    await client.ConnectAsync(new Uri(url), context.RequestAborted);
                }
                catch (WebSocketException)
                {
                    context.Response.StatusCode = 400;
                    return;
                }

                using (var server = await context.WebSockets.AcceptWebSocketAsync(client.SubProtocol))
                {
                    await Task.WhenAll(PumpWebSocket(context, client, server, bufferSize, context.RequestAborted), PumpWebSocket(context, server, client, bufferSize, context.RequestAborted));
                }
            }
        }

        /// <summary>
        /// Core pump method
        /// </summary>
        /// <param name="context"></param>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="_options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task PumpWebSocket(HttpContext context, WebSocket source, WebSocket destination, int? bufferSize, CancellationToken cancellationToken)
        {


            var buffer = new byte[bufferSize ?? DefaultBufferSize];
            while (true)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await source.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    await destination.CloseOutputAsync(WebSocketCloseStatus.EndpointUnavailable, null, cancellationToken);
                    return;
                }
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await destination.CloseOutputAsync(source.CloseStatus.Value, source.CloseStatusDescription, cancellationToken);
                    return;
                }

                await destination.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, cancellationToken);
            }
        }
    }
}
