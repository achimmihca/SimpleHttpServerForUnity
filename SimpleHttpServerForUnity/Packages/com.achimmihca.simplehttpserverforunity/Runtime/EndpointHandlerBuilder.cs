using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SimpleHttpServerForUnity
{
    public class EndpointHandlerBuilder
    {
        private readonly HttpServer httpServer;
        private readonly HttpMethod httpMethod;
        private readonly string pathPattern;
        private string description;
        private Action<EndpointRequestData> requestCallback;
        private GameObject gameObjectForOnDestroy;
        private ResponseThread responseThread;
        
        public EndpointHandlerBuilder(HttpServer httpServer, HttpMethod httpMethod, string pathPattern)
        {
            this.httpServer = httpServer;
            this.httpMethod = httpMethod;
            this.pathPattern = pathPattern;
        }

        public EndpointHandlerBuilder WithDescription(string description)
        {
            if (this.description != null)
            {
                throw new ArgumentException("Description already set");
            }
            
            this.description = description;
            return this;
        }

        public EndpointHandlerBuilder OnThread(ResponseThread responseThread)
        {
            this.responseThread = responseThread;
            return this;
        }
        
        public EndpointHandlerBuilder UntilDestroy(GameObject gameObject)
        {
            if (this.gameObjectForOnDestroy != null)
            {
                throw new ArgumentException("GameObject for OnDestroy already set");
            }
            
            this.gameObjectForOnDestroy = gameObject;
            return this;
        }
        
        public void Do(Action<EndpointRequestData> requestCallback)
        {
            if (this.requestCallback != null)
            {
                throw new ArgumentException("Request callback already set");
            }
            
            this.requestCallback = requestCallback;
            if (gameObjectForOnDestroy != null)
            {
                RemoveEndpointOnDestroy removeEndpointOnDestroy = gameObjectForOnDestroy.AddComponent<RemoveEndpointOnDestroy>();
                removeEndpointOnDestroy.httpMethod = httpMethod;
                removeEndpointOnDestroy.pathPattern = pathPattern;
                removeEndpointOnDestroy.httpServer = httpServer;
            }
            
            httpServer.RegisterEndpoint(new EndpointHandler(httpMethod, pathPattern, description, responseThread, requestCallback));
        }
    }
}
