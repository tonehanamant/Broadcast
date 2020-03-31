using log4net;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BroadcastLogging
{
    /// <summary>
    /// Log message handler for logging web requests and responses.
    /// </summary>
    /// <seealso cref="System.Net.Http.DelegatingHandler" />
    public class BroadcastWebLogMessageHandler : DelegatingHandler
    {
        private ILog _Log;

        public BroadcastWebLogMessageHandler(ILog log)
        {
            _Log = log;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // this allows us to link the request and the response.
            var corrId = Guid.NewGuid();
            var requestInfo = string.Format("{0} {1}", request.Method, request.RequestUri);

            var requestMessage = await request.Content.ReadAsByteArrayAsync();

            await IncommingMessageAsync(corrId, requestInfo, requestMessage);

            var response = await base.SendAsync(request, cancellationToken);

            byte[] responseMessage;

            if (response.IsSuccessStatusCode && (response.Content != null))
                responseMessage = await response.Content.ReadAsByteArrayAsync();
            else
                responseMessage = Encoding.UTF8.GetBytes(response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
            {
                await OutgoingMessageSuccessAsync(corrId, requestInfo, responseMessage);
            }
            else
            {
                await OutgoingMessageErrorAsync(corrId, requestInfo, responseMessage);
            }

            return response;
        }

        protected async Task IncommingMessageAsync(Guid correlationId, string requestInfo, byte[] message)
        {
            await Task.Run(() =>
                LogWebRequest(correlationId, requestInfo, Encoding.UTF8.GetString(message)));
        }

        protected async Task OutgoingMessageSuccessAsync(Guid correlationId, string requestInfo, byte[] message)
        {
            await Task.Run(() =>
                LogWebResponseInfo(correlationId, requestInfo, Encoding.UTF8.GetString(message)));
        }

        protected async Task OutgoingMessageErrorAsync(Guid correlationId, string requestInfo, byte[] message)
        {
            await Task.Run(() =>
                LogWebResponseError(correlationId, requestInfo, Encoding.UTF8.GetString(message)));
        }

        protected void LogWebRequest(Guid correlationId, string requestInfo, string message)
        {
            var logMessage = BroadcastLogMessageHelper.GetHttpRequestLogMessage(message, correlationId, requestInfo);
            _Log.Info(logMessage.ToJson());
        }

        protected void LogWebResponseInfo(Guid correlationId, string requestInfo, string message)
        {
            var logMessage = BroadcastLogMessageHelper.GetHttpResponseLogMessage(message, correlationId, requestInfo);
            _Log.Info(logMessage.ToJson());
        }

        protected void LogWebResponseError(Guid correlationId, string requestInfo, string message)
        {
            var logMessage = BroadcastLogMessageHelper.GetHttpResponseLogMessage(message, correlationId, requestInfo);
            _Log.Error(logMessage.ToJson());
        }
    }
}