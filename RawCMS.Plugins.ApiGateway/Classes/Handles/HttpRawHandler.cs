﻿using Microsoft.AspNetCore.Http;
using RawCMS.Plugins.ApiGateway.Classes.Settings;
using RawCMS.Plugins.ApiGateway.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RawCMS.Plugins.ApiGateway.Classes.Handles
{
    public class HttpRawHandler : RawHandler
    {
        public override HandlerProtocolType HandlerRequestType => HandlerProtocolType.Http;

        public override async Task HandleRequest(HttpContext context, Node node, int? bufferSize = null, bool chunked = false, TimeSpan? keepAlive = null)
        {
            var requestMessage = new HttpRequestMessage();
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) && !HttpMethods.IsHead(requestMethod) && 
                !HttpMethods.IsDelete(requestMethod) && !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            // All request headers and cookies must be transferend to remote server. Some headers will be skipped
            foreach (var header in context.Request.Headers)
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }


            requestMessage.Headers.Host = node.Host;
            //recreate remote url
            string uriString = GetUri(context, node.Host, node.Port, node.Scheme);
            requestMessage.RequestUri = new Uri(uriString);
            requestMessage.Method = new HttpMethod(context.Request.Method);
            var _httpClient = new HttpClient();
            using (var responseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
            {
                context.Response.StatusCode = (int)responseMessage.StatusCode;
                foreach (var header in responseMessage.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                foreach (var header in responseMessage.Content.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }


                if (!chunked)
                {
                    //tell to the browser that response is not chunked
                    context.Response.Headers.Remove("transfer-encoding");
                    await responseMessage.Content.CopyToAsync(context.Response.Body);
                }
                else
                {
                    var buffer = new byte[bufferSize ?? DefaultBufferSize];

                    using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        int len = 0;
                        int full = 0;
                        while ((len = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                            full += buffer.Length;
                        }

                        context.Response.Headers.Remove("transfer-encoding");
                    }
                }
            }
        }
    }
}
