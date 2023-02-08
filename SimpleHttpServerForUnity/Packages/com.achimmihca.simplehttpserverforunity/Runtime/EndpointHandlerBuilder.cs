using System;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;

namespace SimpleHttpServerForUnity
{
    public class EndpointHandlerBuilder
    {
        public HttpServer HttpServer {get; private set; }
        public HttpMethod HttpMethod {get; private set; }
        public string PathPattern {get; private set; }
        public string Description {get; private set; }
        public Action<EndpointRequestData> RequestCallback {get; private set; }
        public Func<EndpointRequestData, bool> ConditionFunction {get; private set; }
        public GameObject GameObjectForOnDestroy {get; private set; }
        public ResponseThread ResponseThread {get; private set; }
        public Dictionary<object, object> UserData { get; private set; } = new();

        public EndpointHandlerBuilder(HttpServer httpServer, HttpMethod httpMethod, string pathPattern, ResponseThread responseThread)
        {
            this.HttpServer = httpServer;
            this.HttpMethod = httpMethod;
            this.PathPattern = pathPattern;
            this.ResponseThread = responseThread;
        }

        public EndpointHandlerBuilder SetDescription(string newDescription)
        {
            if (this.Description != null)
            {
                throw new ArgumentException("Description already set");
            }
            
            this.Description = newDescription;
            return this;
        }

        public EndpointHandlerBuilder SetThread(ResponseThread newResponseThread)
        {
            this.ResponseThread = newResponseThread;
            return this;
        }

        public EndpointHandlerBuilder SetUserData(Dictionary<object, object> newUserData)
        {
            this.UserData = newUserData;
            return this;
        }

        public EndpointHandlerBuilder SetRemoveOnDestroy(GameObject gameObject)
        {
            if (this.GameObjectForOnDestroy != null)
            {
                throw new ArgumentException("GameObject for OnDestroy already set");
            }
            
            this.GameObjectForOnDestroy = gameObject;
            return this;
        }

        public EndpointHandlerBuilder SetCallback(Action<EndpointRequestData> newRequestCallback)
        {
            if (this.RequestCallback != null)
            {
                throw new ArgumentException("Request callback already set");
            }
            
            this.RequestCallback = newRequestCallback;
            return this;
        }

        public EndpointHandlerBuilder SetCondition(Func<EndpointRequestData, bool> newConditionFunction)
        {
            if (this.ConditionFunction != null)
            {
                throw new ArgumentException("Condition function already set");
            }
            
            this.ConditionFunction = newConditionFunction;
            return this;
        }
        
        public void Add()
        {
            if (GameObjectForOnDestroy != null)
            {
                RemoveEndpointOnDestroy removeEndpointOnDestroy = GameObjectForOnDestroy.AddComponent<RemoveEndpointOnDestroy>();
                removeEndpointOnDestroy.httpMethod = HttpMethod;
                removeEndpointOnDestroy.pathPattern = PathPattern;
                removeEndpointOnDestroy.httpServer = HttpServer;
            }
            
            HttpServer.RegisterEndpoint(new EndpointHandler(HttpMethod, PathPattern, Description, ResponseThread, RequestCallback, ConditionFunction, UserData));
        }
    }
}
