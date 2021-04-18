using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace SimpleHttpServerForUnity
{
    public class EndpointHandler
    {
        public static Comparison<EndpointHandler> CompareDescendingByPlaceholderCount { get; private set; } =
            (a, b) => b.PlaceholderCount.CompareTo(a.PlaceholderCount);

        public HttpMethod HttpMethod => endpointData.HttpMethod;
        public string PathPattern => endpointData.PathPattern;
        public string Description => endpointData.Description;
        public int PlaceholderCount => patternMatcher.PlaceholderCount;
        
        private readonly EndpointData endpointData;
        private readonly Action<EndpointRequestData> requestCallback;
        private readonly CurlyBracePlaceholderMatcher patternMatcher;

        public EndpointHandler(HttpMethod httpMethod, string pathPattern, string description, Action<EndpointRequestData> requestCallback)
        {
            this.patternMatcher = new CurlyBracePlaceholderMatcher(pathPattern);
            this.endpointData = new EndpointData(httpMethod, pathPattern, description);
            this.requestCallback = requestCallback;
        }

        public bool TryHandle(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            if (string.Equals(request.HttpMethod, HttpMethod.Method, StringComparison.OrdinalIgnoreCase)
                && patternMatcher.TryMatch(request.Url.AbsolutePath, out Dictionary<string, string> placeholderValues))
            {
                EndpointRequestData endpointRequestData = new EndpointRequestData(context, placeholderValues);
                requestCallback(endpointRequestData);
                return true;
            }

            return false;
        }
    }
}
