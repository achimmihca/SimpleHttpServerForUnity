using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using UnityEngine;

namespace SimpleHttpServerForUnity
{
    public static class HttpServerExtensions
    {
        public static void RemoveEndpoint(this HttpServer httpServer, EndpointHandler endpointHandler)
        {
            httpServer.RemoveEndpoint(endpointHandler.HttpMethod, endpointHandler.PathPattern);
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
}
