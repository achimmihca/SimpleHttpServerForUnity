using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using UnityEngine;

namespace SimpleHttpServerForUnity
{
    public class HttpServer : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitOnLoad()
        {
            instance = null;
        }

        private static HttpServer instance;
        public static HttpServer Instance {
            get
            {
                if (instance == null)
                {
                    HttpServer instanceInScene = FindObjectOfType<HttpServer>();
                    if (instanceInScene != null)
                    {
                        instanceInScene.InitSingleInstance();
                    }
                }
                return instance;
            }
        }
        
        public static bool IsSupported => HttpListener.IsSupported;
        
        public string scheme = "http";
        // Note: IP address of the current device is available via IpAddressUtils.GetIpAddress(AddressFamily.IPv4)
        public string host = "localhost";
        public int port = 6789;

        public Action<EndpointRequestData> NoEndpointFoundCallback { get; set; } = DefaultNoEndpointFoundCallback;
        
        private HttpListener httpListener;
        private bool hasBeenDestroyed;

        private readonly Dictionary<string, EndpointHandler> idToEndpointHandlerMap = new Dictionary<string, EndpointHandler>();
        private readonly List<EndpointHandler> sortedEndpointHandlers = new List<EndpointHandler>();

        private readonly ConcurrentQueue<PendingRequest> requestQueue = new ConcurrentQueue<PendingRequest>();

        private Thread acceptRequestThread;

        protected virtual void Awake()
        {
            InitSingleInstance();
            if (instance != this)
            {
                return;
            }
            
            if (!IsSupported)
            {
                Debug.LogWarning("HttpListener not supported on this platform");
            }
        }

        protected virtual void OnDestroy()
        {
            hasBeenDestroyed = true;
            if (httpListener != null && httpListener.IsListening)
            {
                StopHttpListener();
            }
        }
        
        protected virtual void Update()
        {
            // Process the requests in Update. Update is called from Unity's main thread which allows access to all Unity API.
            while (requestQueue.TryDequeue(out PendingRequest pendingRequest))
            {
                DoHandleRequest(pendingRequest);
            }
        }

        public void StartHttpListener()
        {
            if (!IsSupported)
            {
                Debug.LogError("HttpListener not supported on this platform.");
                return;
            }
            
            if (httpListener != null && httpListener.IsListening)
            {
                Debug.LogWarning("HttpServer already listening");
                return;
            }
            
            if (httpListener == null)
            {
                httpListener = new HttpListener();
                httpListener.Prefixes.Add($"{scheme}://{host}:{port}/");                
            }
            
            Debug.Log($"Starting HttpListener on {host}:{port}");
            httpListener.Start();

            acceptRequestThread = new Thread(() => 
            {
                while (!hasBeenDestroyed && httpListener != null && httpListener.IsListening)
                {
                    // Blocking call, no busy waiting.
                    AcceptRequest(httpListener);
                }
            });
            acceptRequestThread.Start();
        }

        public void StopHttpListener()
        {
            if (httpListener == null || !httpListener.IsListening)
            {
                Debug.LogWarning("HttpServer already not listening");
                return;
            }
            
            Debug.Log($"Stopping HttpListener on {host}:{port}");
            httpListener?.Close();
            httpListener = null;
        }

        public EndpointHandlerBuilder On(HttpMethod httpMethod, string pathPattern)
        {
            return new EndpointHandlerBuilder(this, httpMethod, pathPattern);
        }
        
        public void RegisterEndpoint(EndpointHandler endpointHandler)
        public virtual void RegisterEndpoint(EndpointHandler endpointHandler)
        {
            if (!IsSupported)
            {
                return;
            }

            string endpointId = GetEndpointId(endpointHandler.HttpMethod, endpointHandler.PathPattern);
            if (idToEndpointHandlerMap.ContainsKey(endpointId))
            {
                this.RemoveEndpoint(endpointHandler);
            }

            idToEndpointHandlerMap.Add(endpointId, endpointHandler);
            sortedEndpointHandlers.Add(endpointHandler);
            sortedEndpointHandlers.Sort(EndpointHandler.CompareDescendingByPlaceholderCountThenSegmentCount);
        }

        public virtual void RemoveEndpoint(HttpMethod httpMethod, string pathPattern)
        {
            string endpointId = GetEndpointId(httpMethod, pathPattern);
            if (idToEndpointHandlerMap.TryGetValue(endpointId, out EndpointHandler endpointHandler))
            {
                idToEndpointHandlerMap.Remove(endpointId);
                sortedEndpointHandlers.Remove(endpointHandler);
            }
        }

        public virtual List<EndpointData> GetRegisteredEndpoints()
        {
            return sortedEndpointHandlers
                .Select(it => new EndpointData(it.HttpMethod, it.PathPattern, it.Description))
                .ToList();
        }
        
        protected virtual void AcceptRequest(HttpListener listener)
        {
            HttpListenerContext context;
            try
            {
                // Note: The GetContext method blocks while waiting for a request.
                context = listener.GetContext();

                if (TryFindMatchingEndpointHandler(context, out PendingRequest pendingRequest))
                {
                    if (pendingRequest.EndpointHandler.ResponseThread == ResponseThread.Immediately)
                    {
                        // Handle the request immediately
                        DoHandleRequest(pendingRequest);
                    }
                    if (pendingRequest.EndpointHandler.ResponseThread == ResponseThread.NewThread)
                    {
                        // Handle the request on a new thread
                        new Thread(() => DoHandleRequest(pendingRequest)).Start();
                    }
                    else
                    {
                        // The Request is enqueued and processed on the main thread (i.e. in Update).
                        // This enables access to all Unity API in the callback.
                        requestQueue.Enqueue(pendingRequest);
                    }
                }
                else
                {
                    // No matching handler. This is handled on the main thread.
                    requestQueue.Enqueue(new PendingRequest(context, null, null));
                }
            }
            catch (Exception e)
            {
                if (e is HttpListenerException hle
                    && hle.ErrorCode == 500
                    && hasBeenDestroyed)
                {
                    // Dont log error when closing the HttpListener has interrupted a blocking call.
                    return;
                }
                Debug.LogException(e);
            }
        }

        protected virtual void DoHandleRequest(PendingRequest pendingRequest)
        {
            try
            {
                if (pendingRequest.EndpointHandler != null)
                {
                    pendingRequest.EndpointHandler.DoHandle(pendingRequest.HttpListenerContext, pendingRequest.PlaceholderValues);
                    return;
                }
                
                if (NoEndpointFoundCallback != null)
                {
                    NoEndpointFoundCallback(new EndpointRequestData(pendingRequest.HttpListenerContext, null));
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                // Close the output stream.
                try
                {
                    if (pendingRequest.HttpListenerContext != null)
                    {
                        pendingRequest.HttpListenerContext.Response.Close();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception while trying to close the HttpListenerContext.Response.OutputStream");
                    Debug.LogException(e);
                }
            }
        }

        protected virtual bool TryFindMatchingEndpointHandler(HttpListenerContext context, out PendingRequest pendingRequest)
        {
            // The list is already sorted. Thus, the first matching handler is the best matching handler.
            foreach (EndpointHandler handler in sortedEndpointHandlers)
            {
                if (handler.CanHandle(context, out Dictionary<string, string> placeholderValues))
                {
                    pendingRequest = new PendingRequest(context, handler, placeholderValues);
                    return true;
                }
            }

            pendingRequest = null;
            return false;
        }

        protected virtual void InitSingleInstance()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            if (instance != null
                && instance != this)
            {
                // This instance is not needed.
                Destroy(gameObject);
                return;
            }
            instance = this;
            
            // Move object to top level in scene hierarchy.
            // Otherwise this object will be destroyed with its parent, even when DontDestroyOnLoad is used.
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        protected static string GetEndpointId(HttpMethod method, string pattern)
        {
            return method.Method + "|" + pattern;
        }

        protected static void DefaultNoEndpointFoundCallback(EndpointRequestData requestData)
        {
            Debug.Log($"No matching endpoint found for '{requestData.Context.Request.HttpMethod}' on {requestData.Context.Request.Url}");
            requestData.Context.Response.SendResponse("", HttpStatusCode.NotFound);
        }
    }
}
