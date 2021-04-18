using System.Net.Http;

namespace SimpleHttpServerForUnity
{
    public class EndpointData
    {
        public HttpMethod HttpMethod { get; private set; }
        public string PathPattern { get; private set; }
        public string Description { get; private set; }

        public EndpointData(HttpMethod httpMethod, string pathPattern, string description)
        {
            HttpMethod = httpMethod;
            PathPattern = pathPattern;
            Description = description;
        }
    }
}
