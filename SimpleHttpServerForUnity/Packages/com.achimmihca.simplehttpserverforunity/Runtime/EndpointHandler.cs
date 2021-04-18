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
        public ResponseThread ResponseThread { get; private set; } = ResponseThread.MainThread;
        
        private readonly EndpointData endpointData;
        private readonly Action<EndpointRequestData> requestCallback;
        private readonly CurlyBracePlaceholderMatcher patternMatcher;

        public EndpointHandler(
            HttpMethod httpMethod,
            string pathPattern,
            string description,
            ResponseThread responseThread,
            Action<EndpointRequestData> requestCallback)
        {
            this.patternMatcher = new CurlyBracePlaceholderMatcher(pathPattern);
            this.endpointData = new EndpointData(httpMethod, pathPattern, description);
            this.ResponseThread = responseThread;
            this.requestCallback = requestCallback;
        }

        public bool CanHandle(HttpListenerContext context, out Dictionary<string, string> placeholderValues)
        {
            HttpListenerRequest request = context.Request;
            placeholderValues = null;
            return string.Equals(request.HttpMethod, HttpMethod.Method, StringComparison.OrdinalIgnoreCase)
                   && patternMatcher.TryMatch(request.Url.AbsolutePath, out placeholderValues);
        }

        public void DoHandle(HttpListenerContext context, Dictionary<string, string> placeholderValues)
        {
            EndpointRequestData endpointRequestData = new EndpointRequestData(context, placeholderValues);
            requestCallback(endpointRequestData);
        }
    }
}
