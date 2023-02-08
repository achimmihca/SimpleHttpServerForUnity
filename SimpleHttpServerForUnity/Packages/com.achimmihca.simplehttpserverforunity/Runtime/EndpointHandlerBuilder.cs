using System;
using System.Net.Http;
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
        private Func<EndpointRequestData, bool> conditionFunction;
        private GameObject gameObjectForOnDestroy;
        private ResponseThread responseThread;
        
        public EndpointHandlerBuilder(HttpServer httpServer, HttpMethod httpMethod, string pathPattern, ResponseThread responseThread)
        {
            this.httpServer = httpServer;
            this.httpMethod = httpMethod;
            this.pathPattern = pathPattern;
            this.responseThread = responseThread;
        }

        public EndpointHandlerBuilder SetDescription(string description)
        {
            if (this.description != null)
            {
                throw new ArgumentException("Description already set");
            }
            
            this.description = description;
            return this;
        }

        public EndpointHandlerBuilder SetThread(ResponseThread responseThread)
        {
            this.responseThread = responseThread;
            return this;
        }
        
        public EndpointHandlerBuilder SetRemoveOnDestroy(GameObject gameObject)
        {
            if (this.gameObjectForOnDestroy != null)
            {
                throw new ArgumentException("GameObject for OnDestroy already set");
            }
            
            this.gameObjectForOnDestroy = gameObject;
            return this;
        }

        public EndpointHandlerBuilder SetCallback(Action<EndpointRequestData> requestCallback)
        {
            if (this.requestCallback != null)
            {
                throw new ArgumentException("Request callback already set");
            }
            
            this.requestCallback = requestCallback;
            return this;
        }

        public EndpointHandlerBuilder SetCondition(Func<EndpointRequestData, bool> conditionFunction)
        {
            if (this.conditionFunction != null)
            {
                throw new ArgumentException("Condition function already set");
            }
            
            this.conditionFunction = conditionFunction;
            return this;
        }
        
        public void Add()
        {
            if (gameObjectForOnDestroy != null)
            {
                RemoveEndpointOnDestroy removeEndpointOnDestroy = gameObjectForOnDestroy.AddComponent<RemoveEndpointOnDestroy>();
                removeEndpointOnDestroy.httpMethod = httpMethod;
                removeEndpointOnDestroy.pathPattern = pathPattern;
                removeEndpointOnDestroy.httpServer = httpServer;
            }
            
            httpServer.RegisterEndpoint(new EndpointHandler(httpMethod, pathPattern, description, responseThread, requestCallback, conditionFunction));
        }
    }
}
