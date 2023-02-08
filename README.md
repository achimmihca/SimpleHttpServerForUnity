[![Build Status](https://travis-ci.org/achimmihca/SimpleHttpServerForUnity.svg?branch=main)](https://travis-ci.org/achimmihca/SimpleHttpServerForUnity)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/achimmihca/SimpleHttpServerForUnity/blob/main/LICENSE)
[![Sponsor this project](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/achimmihca)

# SimpleHttpServerForUnity

A simple HTTP server for Unity3D.

- Register endpoints and serve requests with ease.
- Respond to requests from Unity's main thread, from a new thread, or immediately.
- The implementation builds on C# [HttpListener](https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener?view=net-5.0).

# How to Use

## Get the Package

- You can add a dependency to your `Packages/manifest.json` using a [Git URL](https://docs.unity3d.com/2019.4/Documentation/Manual/upm-git.html) in the following form:
  `"com.achimmihca.simplehttpserverforunity": "https://github.com/achimmihca/SimpleHttpServerForUnity.git?path=SimpleHttpServerForUnity/Packages/com.achimmihca.simplehttpserverforunity#v1.0.0"`
  - Note that `#v1.0.0` can be used to specify a tag or commit hash.
- This package requires `Api Compatibility Level` `.NET 4.x` or above (in Unity `File > Project Settings... > Player > Other settings`)
- This package ships with a sample that can be imported to your project using Unity's Package Manager.

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
List<string> endpointInfos = httpServer.GetRegisteredEndpoints()
    .Select(endpoint => $"{endpoint.HttpMethod} {endpoint.PathPattern} - {endpoint.Description}")
    .ToList();
Debug.Log("Registered endpoints:\n" + string.Join("\n", endpointInfos));
```

# History
SimpleHttpServerForUnity has been created originally for [UltraStar Play](https://github.com/UltraStar-Deluxe/Play) to serve requests from a handful of clients in the same local area network.
If you like singing, karaoke, or sing-along games then go check it out ;)
