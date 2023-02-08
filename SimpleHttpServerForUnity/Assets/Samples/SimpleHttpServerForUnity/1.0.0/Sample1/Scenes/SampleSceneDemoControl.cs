using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using SimpleHttpServerForUnity;
using UnityEngine;

public class SampleSceneDemoControl : MonoBehaviour
{
    public HttpServer httpServer;
    
    void Start()
    {
        // Set host and port before starting the server.
        // httpServer.port = 1234;
        
        // Find the IP address of this device using IpAddressUtils.
        // In this example, we only search for WLAN network adapters.
        httpServer.host = IpAddressUtils.GetIpAddress(AddressFamily.IPv4, NetworkInterfaceType.Wireless80211);
        
        // Register endpoints
        
        // For example, matches a GET request on '/hello/Alice'
        httpServer.CreateEndpoint(HttpMethod.Get, "/hello/{name}")
            .SetDescription("Say hello to someone") // Optionally add a description
            .SetThread(ResponseThread.MainThread) // Optionally handle requests on the main thread or immediately
            .SetRemoveOnDestroy(gameObject) // Optionally remove endpoint on destroy of some GameObject
            .SetCondition(HasDummyPermission) // Optionally with condition
            .SetCallback(HandleHelloRequest)
            .Add();

        // Print registered endpoints
        List<string> endpointInfos = httpServer.GetRegisteredEndpoints()
            .Select(endpoint => $"{endpoint.HttpMethod} {endpoint.PathPattern} - {endpoint.Description}")
            .ToList();
        Debug.Log("Registered endpoints: \n" + string.Join("\n", endpointInfos));
        
        // Start the server to answer requests
        httpServer.StartHttpListener();
    }

    private bool HasDummyPermission(EndpointRequestData requestData)
    {
        if (requestData.Context.Request.Headers["permissions"] == "dummy")
        {
            return true;
        }

        requestData.Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        requestData.Context.Response.ContentType = "text/plain";
        requestData.Context.Response.OutputStream.Write(Encoding.UTF8.GetBytes("Missing dummy permission"));
        return false;
    }

    private void HandleHelloRequest(EndpointRequestData requestData)
    {
        List<string> pathParameterPairs = requestData.PathParameters
            .Select(it => $"{it.Key}={it.Value}")
            .ToList();
            
        List<string> queryParameterPairs = requestData.Context.Request.QueryString
            .ToDictionary()
            .Select(it => $"{it.Key}={it.Value}")
            .ToList();
        
        Debug.Log($"Received request.\n" +
                  $"Path parameters: {string.Join(" | ", pathParameterPairs)}\n" +
                  $"Query parameters: {string.Join(" | ", queryParameterPairs)}");

        // Send JSON response
        requestData.Context.Response.SendResponse("{\"message\":\"Hello " + requestData.PathParameters["name"] + "\"}", HttpStatusCode.OK);        
    }
}
