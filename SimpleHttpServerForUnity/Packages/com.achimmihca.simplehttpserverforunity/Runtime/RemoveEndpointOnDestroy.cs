using System.Net.Http;
using UnityEngine;

namespace SimpleHttpServerForUnity
{
    public class RemoveEndpointOnDestroy : MonoBehaviour
    {
        public HttpServer httpServer;
        public HttpMethod httpMethod;
        public string pathPattern;
    
        private void OnDestroy()
        {
            if (httpServer != null)
            {
                httpServer.RemoveEndpoint(httpMethod, pathPattern);
            }
        }
    }
}
