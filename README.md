# FintX
FintX is a native, cross-platform, gRPC test bench.  It not only works on Windows, MacOS and Linux but any data files created are portable to all platforms.  

Extremely honored to have FintX featured in the Avalonia App Showcase https://avaloniaui.net/Showcase

<img src="https://github.com/namigop/FintX/blob/main/docs/ServerStreaming.png" style="width: 80%; height: 80%" />

## Download FintX

FintX Enterprise reverts to the Community Edition once the trial period is over.  FintX Community is free forever

- Download for windows ([Windows Store](https://apps.microsoft.com/detail/9nz2g6flp92q?hl=en-US&gl=US))
- Download for macos ([Apple Silicon](https://github.com/namigop/fintx-arm64/releases/download/arm64-v3.0.7/FintX-osx-Setup.pkg))
- Download for [linux x64](https://github.com/namigop/fintx-linux/releases/download/linux-v3.0.7/FintX.AppImage)


## Looking for Advanced Features?

Check out the FintX Enterprise version at [https://fintx.dev](https://fintx.dev)

| **GRPC Service Discovery**                                               | Community | Enterprise |
|--------------------------------------------------------------------------|-----------|-----------|
| Discovery using *.proto file                                             | ☑         | ☑ |
| Discovery using server reflection                                        | ☑         | ☑ |
| Generates both synchronous and asynchronous method                       | ☑         | ☑ |
|                                                                          |           |  |
| **GRPC Service Method Testing**                                          | Community | Enterprise |
| Support for Unary                                                        | ☑         | ☑ |
| Support for Server Streaming                                             | ☑         | ☑ |
| Support for Client Streaming                                             | ☑         | ☑ |
| Support for Bi-directional (Duplex) Streaming                            | ☑         | ☑ |
| Automatic generation of requests                                         | ☑         | ☑ |
| Provides an easy-to-use object editor for editing requests               | ☑         | ☑ |
| Support for call credentials                                             | ☑         | ☑ |
| Support for certificates                                                 | ☑         | ☑ |
| Support for cancelling long-running streaming calls                      | ☑         | ☑ |
|                                                                          |           |  |
| **Request Data Management**                                              | Community | Enterprise |
| Import and export of individual requests                                 | ☑         | ☑ |
| Create new requests and save into a project                              | ☑         | ☑ |
| Auto-save of open tabs                                                   | ☑         | ☑ |
| Reopens the previous session                                             | ☑         | ☑ |
| Git-aware. Files can be tracked in git                                   | ☑         | ☑ |
|                                                                          |           |  |
| **Functional Testing**                                                   | Community | Enterprise |
| Create and manage functional tests                                       | -         | ☑ |
| Show visual diff of expected vs actual responses                         | -         | ☑ |
| Supports functional tests for Unary, Client, Server and Duplex streaming | -         | ☑ |
| Generate functional test reports (in markdown)                           | -         | ☑ |
| Auto-save of functional tests                                            | -         | ☑ |
| Edit test reports                                                        | -         | ☑ |
|                                                                          | -         |  |
| **Test Collections**                                                     | Community | Enterprise |
| Create and manage test collections                                       | -         | ☑ |
| Execute tests in sequence or in parallel                                 | -         | ☑ |
| Generate test collection reports (in markdown)                           | -         | ☑ |
| Edit test reports                                                        | -         | ☑ |
|                                                                          |           |  |
| **Performance Testing**                                                  | Community | Enterprise |
| Create and manage performance tests                                      | -         | ☑ |
| Supports varius load types (Incremental, Burst, etc,)                    | -         | ☑ |
| Real-time view of performance metrics                                    | -         | ☑ |


## Quickstart
1. Add a client
    - Select a proto file or enter the http address of the gRPC reflection service
    - Enter a name for the gRPC client
   
   <img src="https://github.com/namigop/FintX/blob/main/docs/AddClient.png" style="width: 70%; height: 70%">
   
3. `Click Okay`.  This will generate code for the gRPC client.
   - The generated code is then compiled into an assembly
   - The generated assembly is inspected and client methods are displayed in the explorer tree (left-side)
     
4. `Double-click on the method` to open it in a tab
5. `Double click on a node` on the request tree to edit it's value

   <img src="https://github.com/namigop/FintX/blob/main/docs/EditRequest.png" style="width: 70%; height: 70%">   

6. Click the Run button to invoke the gRPC service

   <img src="https://github.com/namigop/FintX/blob/main/docs/RequestResponse.png" style="width: 70%; height: 70%">

