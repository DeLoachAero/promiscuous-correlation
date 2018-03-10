using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Linq;

namespace DeLoachAero.WebApi
{
    /// <summary>
    /// Collection of correlation ID HTTP headers with values
    /// </summary>
    public class CorrelationIdHeaderCollection : HttpHeaders
    {
        /// <summary>
        /// List of HTTP header names that are required to be included in response headers,
        /// or request headers to downstream services.
        /// </summary>
        public List<string> RequiredHeaders = new List<string>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public CorrelationIdHeaderCollection()
        { }

        /// <summary>
        /// Constructor that copies an HTTP header collection
        /// </summary>
        public CorrelationIdHeaderCollection(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            this.Concat(headers);
            foreach (var h in headers)
                RequiredHeaders.Add(h.Key);
        }

        /// <summary>
        /// Create a copy of the correlation header collection that includes only the subset
        /// of headers marked as required for responses and downstream requests
        /// </summary>
        /// <returns></returns>
        public CorrelationIdHeaderCollection GetRequiredHeaderCollection()
        {
            return new CorrelationIdHeaderCollection(this.Where(a => RequiredHeaders.Contains(a.Key)));
        }
    }
}
