[![Build Status](https://travis-ci.org/achimmihca/SimpleHttpServerForUnity.svg?branch=main)](https://travis-ci.org/achimmihca/SimpleHttpServerForUnity)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/achimmihca/SimpleHttpServerForUnity/blob/main/LICENSE)

# SimpleHttpServerForUnity

A simple HTTP server for Unity3D.

- Register endpoints and serve requests with ease.
- Respond to requests from Unity's main thread, from a new thread, or immediately.
- The implementation builds on C# HttpListener.

# How to Use

## Get the Code

- You can add a dependency to your `Packages/manifest.json` in the following form:
  `"com.achimmihca.simplehttpserverforunity": "https://github.com/achimmihca/SimpleHttpServerForUnity.git?path=SimpleHttpServerForUnity/Packages/com.achimmihca.simplehttpserverforunity#v1.0.0"`
    - Note that `#v1.0.0` specifies a tag of this git repository. Remove this part to use the latest (possibly unstable) version.
    - Note further that the path parameter (`?path=...`) points to the folder in this git repository, where the Unity package is placed.
- This package ships with a sample that can be imported to your project using Unity's Package Manager.
- This project requires `Api Compatibility Level` `.NET 4.x` or above (in Unity `File > Project Settings... > Player > Other settings`)

## Prepare HttpServer
- Add an instance of HttpServer component to your scene.

- Set host and port
    - Note: You can find the IP address of the current device using IpAddressUtils.

```
    httpServer.port = 1234;        
    httpServer.host = IpAddressUtils.GetIpAddress(AddressFamily.IPv4, NetworkInterfaceType.Wireless80211);
```

## Register endpoints

```
// Matches a GET request on '/hello/Alice' for example
httpServer.On(HttpMethod.Get, "/hello/{name}")
    .WithDescription("Say hello to someone") // Optionally, add a description
    .OnThread(ResponseThread.MainThread) // Optionally, handle requests on the main thread, a new thread, or immediately
    .UntilDestroy(gameObject) // Optionally, remove endpoint on destroy of some GameObject
    .Do(requestData => HandleHelloRequest(requestData));

// Matches a GET request on '/files/folder/subfolder/image.png' for example
httpServer.On(HttpMethod.Get, "/files/*")
    .Do(requestData => HandleFileRequest(requestData));
```

## Start the Server
```
httpServer.StartHttpListener();
```

## Get Registered Endpoints

```
// Print registered endpoints
List<string> endpointInfos = httpServer.GetRegisteredEndpoints()
    .Select(endpoint => $"{endpoint.HttpMethod} {endpoint.PathPattern} - {endpoint.Description}")
    .ToList();
Debug.Log("Registered endpoints:\n" + string.Join("\n", endpointInfos));
```