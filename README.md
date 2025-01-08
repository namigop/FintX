# FintX
FintX is a native, cross-platform, gRPC test bench.  It not only works on Windows, MacOS and Linux but any data files created are portable to all platforms.  

Extremely honored to have FintX featured in the Avalonia App Showcase https://avaloniaui.net/Showcase

<img src="https://github.com/namigop/FintX/blob/main/docs/ServerStreaming.png" style="width: 80%; height: 80%" />

## Looking for Advanced Features?
If you need to do
- Performance/Load Testing with real-time results
- Functional Testing (Test cases with req / resp comparison)
- Funcitonal Test Collections
- and more..

Check out the FintX Enterprise version at [https://fintx.dev](https://fintx.dev)

## Installation
1. Windows : `winget install FintX`
2. MacOS   : Please see [Installation Guide](https://github.com/namigop/FintX/wiki/Installation-Guide)
3. Linux   : Please see [Installation Guide](https://github.com/namigop/FintX/wiki/Installation-Guide) 

## List of Features
Please see the [feature list](https://github.com/namigop/FintX/wiki/List-of-Features)

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



#### Special Thanks

Special thanks to [JetBrains for supporting open source development](https://jb.gg/OpenSourceSupport) and sponsoring a Rider license!  JetBrains is awesome. If you are doing cross-platform development
for .NET, nothing beats JetBrains Rider

<img src="https://resources.jetbrains.com/storage/products/company/brand/logos/Rider_icon.png" alt="JetBrains Rider" width="96px"/>
