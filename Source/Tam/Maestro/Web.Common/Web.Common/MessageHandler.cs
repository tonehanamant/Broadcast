﻿using Common.Services.WebComponents;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tam.Maestro.Services.Cable
{
    public abstract class MessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var corrId = string.Format("{0}{1}", DateTime.Now.Ticks, Thread.CurrentThread.ManagedThreadId);
            var requestInfo = string.Format("{0} {1}", request.Method, request.RequestUri);

            var requestMessage = await request.Content.ReadAsByteArrayAsync();

            await IncommingMessageAsync(corrId, requestInfo, requestMessage);

            var response = await base.SendAsync(request, cancellationToken);

            byte[] responseMessage;

            if (response.IsSuccessStatusCode && (response.Content != null))
                responseMessage = await response.Content.ReadAsByteArrayAsync();
            else
                responseMessage = Encoding.UTF8.GetBytes(response.ReasonPhrase);

            await OutgoingMessageAsync(corrId, requestInfo, responseMessage);

            return response;
        }

        protected abstract Task IncommingMessageAsync(string correlationId, string requestInfo, byte[] message);
        protected abstract Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message);

        public class MessageLoggingHandler : MessageHandler
        {
            IWebLogger _logger;

            public MessageLoggingHandler(IWebLogger logger)
            {
                _logger = logger;
            }

            protected override async Task IncommingMessageAsync(string correlationId, string requestInfo, byte[] message)
            {
                    await Task.Run(() =>
                        //_logger.LogEventInformation(String.Format("{0} - Request: {1}{2}", correlationId, requestInfo, message)));
                        _logger.LogEventInformation(String.Format("{0} - Request: {1} - Content: {2}", correlationId, requestInfo, Encoding.UTF8.GetString(message)), "TrafficController"));
            }

            protected override async Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message)
            {
                    await Task.Run(() =>
                        //_logger.LogEventInformation(String.Format("{0} - Response: {1}{2}", correlationId, requestInfo, message)));
                        _logger.LogEventInformation(String.Format("{0} - Response: {1} - Content: {2}", correlationId, requestInfo, Encoding.UTF8.GetString(message)), "TrafficController"));
            }
        }
    }
}
