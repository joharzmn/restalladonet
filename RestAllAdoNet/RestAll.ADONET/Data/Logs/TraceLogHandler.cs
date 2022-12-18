using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RESTAll.Data.Logs
{
    internal class TraceLogHandler : DelegatingHandler
    {
        private ILogger _logger;
        public TraceLogHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Request:");
            _logger.LogDebug(request.ToString());
            if (request.Content != null)
            {
                _logger.LogDebug(await request.Content.ReadAsStringAsync());
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            _logger.LogDebug("Response:");

            _logger.LogDebug(response.ToString());
            if (response.Content != null)
            {
                _logger.LogDebug(await response.Content.ReadAsStringAsync());
            }

            return response;
        }
    }
}
