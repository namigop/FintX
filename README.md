# FintX
FintX is a native, cross-platform, gRPC test bench.  It not only works on Windows, MacOS and Linux but any data files created are portable to all platforms

<img src="https://github.com/namigop/FintX/blob/main/docs/ServerStreaming.png" style="width: 80%; height: 80%" />

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


### [Hire me]
Hi there! author of FintX here. I am currently looking for a new position. 

I have held several roles over the years, either as an individual contributor or in a leadership position.  I've been a developer, tech lead, software architect and engineering manager/head. I have led cross-site teams and delivered multi-million budget projects. Apart from C#/F#/.NET (15+ years and counting), I have passing familiarity with Java, Golang, python. If you have a need for someone with a work profile like mine, please drop me an email at erik.araojo@wcfstorm.com


#### Special Thanks

Special thanks to [JetBrains for supporting open source development](https://jb.gg/OpenSourceSupport) and sponsoring a Rider license!  JetBrains is awesome. If you are doing cross-platform development
for .NET, nothing beats JetBrains Rider

<img src="https://resources.jetbrains.com/storage/products/company/brand/logos/Rider_icon.png" alt="JetBrains Rider" width="96px"/>
