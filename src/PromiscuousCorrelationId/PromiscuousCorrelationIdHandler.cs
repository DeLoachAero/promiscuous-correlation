using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DeLoachAero.WebApi
{
    public class PromiscuousCorrelationIdHandler : DelegatingHandler
    {
        /// <summary>
        /// The key name for Request.Properties that holds the incoming header collection
        /// found that contains correlation IDs
        /// </summary>
        public const string CorrelationIdPropertyKey = "PromiscuousCorrelationId";

        /// <summary>
        /// The most often cited header among .NET and Java programs
        /// </summary>
        public const string XCorrelationId = "X-Correlation-Id";
        /// <summary>
        /// Regex template to pick up any dreamed up ids that start with "X-Correlation"...
        /// </summary>
        public const string XCorrelationStar = "^X-Correlation\\S*";
        /// <summary>
        /// Rails-style request ID, per Heroku 
        /// </summary>
        /// <remarks>https://devcenter.heroku.com/articles/http-request-id</remarks>
        public const string XRequestId = "X-Request-Id";
        /// <summary>
        /// From GitHub proposal for hierarchical request ids
        /// </summary>
        /// <remarks>https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.DiagnosticSource/src/HttpCorrelationProtocol.md</remarks>
        public const string RequestId = "Request-Id";
        /// <summary>
        /// Header used by Microsoft.Diagnostics.Correlation library
        /// </summary>
        /// <remarks>https://github.com/Azure/diagnostics-correlation</remarks>
        public const string XMsRequestRootId = "x-ms-request-root-id";
        /// <summary>
        /// Header used by Microsoft.Diagnostics.Correlation library
        /// </summary>
        /// <remarks>https://github.com/Azure/diagnostics-correlation</remarks>
        public const string XMsRequestId = "x-ms-request-id";

        /// <summary>
        /// The list of header patterns and associated bool value that indicates if the
        /// header is required to be present in the list of outgoing headers
        /// (even if it wasn't present in the incoming header list).  By default,
        /// only X-Correlation-Id is required.  You can manipulate this list as desired
        /// for your project.
        /// </summary>
        public static Dictionary<string, bool> HeaderPatterns = new Dictionary<string, bool>
        {
            {XCorrelationId, true },
            {XCorrelationStar, false },
            {XRequestId, false },
            {RequestId, false },
            {XMsRequestRootId, false },
            {XMsRequestId, false }
        };

        /// <summary>
        /// Delegate definition for a method that returns a brand new correlation ID
        /// </summary>
        public delegate string GetNewCorrelationIdDelegate();

        /// <summary>
        /// Delegated method to generate a brand new correlation ID; defaults to a GUID string
        /// </summary>
        public GetNewCorrelationIdDelegate NewIdGenerator = 
            delegate () { return Guid.NewGuid().ToString("D"); };

        /// <summary>
        /// Handler method to extract correlation ID(s) from HTTP header(s)
        /// </summary>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var ids = new CorrelationIdHeaderCollection();

            foreach (var pattern in HeaderPatterns.Keys)
            {
                var isRegex = pattern.StartsWith("^");
                if (!isRegex && HeaderPatterns[pattern])
                    ids.RequiredHeaders.Add(pattern);

                foreach (var h in request.Headers)
                {
                    if ((isRegex && Regex.IsMatch(h.Key, pattern, RegexOptions.IgnoreCase))
                        || (!isRegex && pattern.Equals(h.Key, StringComparison.InvariantCultureIgnoreCase))
                       )
                    {
                        if (!ids.Contains(h.Key))
                        {
                            ids.Add(h.Key, h.Value.Where(a => !String.IsNullOrEmpty(a)));
                            if (isRegex && HeaderPatterns[pattern])
                                ids.RequiredHeaders.Add(h.Key);
                        }
                    }
                }
            }

            // did we find a correlation ID in the request? if not, create one
            string corId;
            if (ids.Count() == 0 || ids.First().Value.Count() == 0)
            {
                corId = NewIdGenerator();
                foreach (var pattern in HeaderPatterns.Keys)
                {
                    if (!pattern.StartsWith("^") && HeaderPatterns[pattern])
                        ids.Add(pattern, corId);
                }
            }
            else
                corId = ids.First().Value.First();

            // add any extra headers to the list that were deemed required but not already set
            foreach (var h in ids.RequiredHeaders)
            {
                if (!ids.Contains(h))
                    ids.Add(h, corId);
            }

            request.Properties[CorrelationIdPropertyKey] = ids;

            var response = await base.SendAsync(request, cancellationToken);

            // add required headers to the outgoing response
            foreach (var h in ids.RequiredHeaders)
            {
                if (!response.Headers.Contains(h))
                    response.Headers.Add(h, ids.GetValues(h));
            }
            return response;
        }
    }
}
