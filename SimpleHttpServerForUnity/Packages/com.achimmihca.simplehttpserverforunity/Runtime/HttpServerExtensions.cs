using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using UnityEngine;

namespace SimpleHttpServerForUnity
{
    public static class HttpServerExtensions
    {
        public static void RegisterEndpoint(this HttpServer httpServer, GameObject gameObject, HttpMethod httpMethod, string pathPattern, string description, Action<EndpointRequestData> requestCallback)
        {
            httpServer.RegisterEndpoint(new EndpointHandler(httpMethod, pathPattern, description, requestCallback));
            
            RemoveEndpointOnDestroy removeEndpointOnDestroy = gameObject.AddComponent<RemoveEndpointOnDestroy>();
            removeEndpointOnDestroy.httpMethod = httpMethod;
            removeEndpointOnDestroy.urlPattern = pathPattern;
            removeEndpointOnDestroy.httpServer = httpServer;
        }
        
        public static void RegisterEndpoint(this HttpServer httpServer, HttpMethod httpMethod, string pathPattern, string description, Action<EndpointRequestData> requestCallback)
        {
            httpServer.RegisterEndpoint(new EndpointHandler(httpMethod, pathPattern, description, requestCallback));
        }

        public static void RemoveEndpoint(this HttpServer httpServer, EndpointHandler endpointHandler)
        {
            httpServer.RemoveEndpoint(endpointHandler.HttpMethod, endpointHandler.UrlPattern);
        }

        public static Dictionary<string, string> ToDictionary(this NameValueCollection nameValueCollection)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (string key in nameValueCollection.AllKeys)
            {
                dictionary[key] = nameValueCollection.Get(key);
            }

            return dictionary;
        }
    }

    public class RemoveEndpointOnDestroy : MonoBehaviour
    {
        public HttpServer httpServer;
        public HttpMethod httpMethod;
        public string urlPattern;
        
        private void OnDestroy()
        {
            if (httpServer != null)
            {
                httpServer.RemoveEndpoint(httpMethod, urlPattern);
            }
        }
    }
}
