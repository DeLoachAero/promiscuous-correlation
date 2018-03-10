using System.Linq;
using System.Net.Http;

namespace DeLoachAero.WebApi
{
    /// <summary>
    /// Static class of extension methods for HttpRequestMessage
    /// </summary>
    public static class RequestCorrelationExtension
    {
        /// <summary>
        /// Empty correlation header collection
        /// </summary>
        private static CorrelationIdHeaderCollection _emptyCollection = new CorrelationIdHeaderCollection();

        /// <summary>
        /// Get the incoming or generated primary correlation ID from the HttpRequestMessage
        /// </summary>
        public static string GetPromiscuousCorrelationId(this HttpRequestMessage request)
        {
            object property;
            if (request.Properties.TryGetValue(PromiscuousCorrelationIdHandler.CorrelationIdPropertyKey, out property))
            {
                var ids = property as CorrelationIdHeaderCollection;
                if (ids == null || ids.Count() == 0)
                    return null;

                return ids.First().Value.First();
            }
            return null;
        }

        /// <summary>
        /// Get the subset of correlation HTTP headers/values that are marked as
        /// required to be sent with HttpResponseMessages, or as request headers
        /// to downstream service requests
        /// </summary>
        public static CorrelationIdHeaderCollection GetRequiredCorrelationHeaders(this HttpRequestMessage request)
        {
            object property;

            if (request.Properties.TryGetValue(PromiscuousCorrelationIdHandler.CorrelationIdPropertyKey, out property))
            {
                var ids = property as CorrelationIdHeaderCollection;
                if (ids == null || ids.Count() == 0)
                    return _emptyCollection;

                return ids.GetRequiredHeaderCollection();
            }
            return _emptyCollection;
        }

    }
}
