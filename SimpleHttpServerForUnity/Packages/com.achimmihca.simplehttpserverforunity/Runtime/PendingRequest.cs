using System.Collections.Generic;
using System.Net;

namespace SimpleHttpServerForUnity
{
    public class PendingRequest
    {
        public HttpListenerContext HttpListenerContext { get; private set; }
        public EndpointHandler EndpointHandler { get; private set; }
        public Dictionary<string, string> PlaceholderValues { get; private set; }

        public PendingRequest(HttpListenerContext httpListenerContext, EndpointHandler endpointHandler, Dictionary<string, string> placeholderValues)
        {
            HttpListenerContext = httpListenerContext;
            EndpointHandler = endpointHandler;
            PlaceholderValues = placeholderValues;
        }
    }
}
