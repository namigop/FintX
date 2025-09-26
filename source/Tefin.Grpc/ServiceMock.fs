namespace Tefin.Grpc

open System.Reflection
open System.Reflection.Emit
open System.Text
open System.Text.RegularExpressions
open System.Threading.Tasks
open Google.Protobuf.WellKnownTypes
open Microsoft.Extensions.DependencyInjection
open System
open Tefin.Core

module ServiceMock =
    let private createDefaultValue (il:ILGenerator) (thisType:Type) =
        if (thisType.IsValueType) then       
            let defaultValue = il.DeclareLocal(thisType)
            il.Emit(OpCodes.Ldloca_S, defaultValue)
            il.Emit(OpCodes.Initobj, thisType)
            il.Emit(OpCodes.Ldloc, defaultValue)         
        else         
            il.Emit(OpCodes.Ldnull)
         
    
    let private createDefaultMethodBody (il:ILGenerator) (returnType:Type) =
        if (returnType = typeof<Task>) then       
            // Return Task.CompletedTask
            let completedTaskProperty = typeof<Task>.GetProperty("CompletedTask");
            il.Emit(OpCodes.Call, completedTaskProperty.GetMethod);        
        else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() = typedefof<Task<_>>) then        
            // Return Task.FromResult(default(T))
            let genericArg = returnType.GetGenericArguments()[0];
            createDefaultValue il genericArg
            
            let fromResultMethod = typeof<Task>.GetMethod("FromResult").MakeGenericMethod(genericArg);
            il.Emit(OpCodes.Call, fromResultMethod);
        
        else        
            // For non-Task return types, return default value
            createDefaultValue il returnType
        
        
        il.Emit(OpCodes.Ret);
    
    let private createDefaultResponseObject (il:ILGenerator) (responseType:Type) =
        try
            let defaultConstructor = responseType.GetConstructor(Type.EmptyTypes)            
            if not (defaultConstructor = null) then                        
                il.Emit(OpCodes.Newobj, defaultConstructor);            
            else            
                // Try to use Activator.CreateInstance for types without parameterless constructor
                il.Emit(OpCodes.Ldtoken, responseType)
                il.Emit(OpCodes.Call, typeof<Type>.GetMethod("GetTypeFromHandle"))
                il.Emit(OpCodes.Call, typeof<Activator>.GetMethod("CreateInstance", [| typeof<Type> |] ))
                il.Emit(OpCodes.Castclass, responseType)
        with exc ->
            il.Emit(OpCodes.Ldnull);
            
    
    let private createUnaryMethod (il:ILGenerator) (returnType:Type) =
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() = typedefof<Task<_>>) then        
            let responseType = returnType.GetGenericArguments()[0]
            createDefaultResponseObject il responseType
            
            let fromResultMethod = typeof<Task>.GetMethod("FromResult").MakeGenericMethod(responseType)
            il.Emit(OpCodes.Call, fromResultMethod);      
        else        
            createDefaultMethodBody il returnType        
        
        il.Emit(OpCodes.Ret)
        
    let private createMethodBody (il:ILGenerator) (method:MethodInfo) =
        let returnType = method.ReturnType
        let parameters = method.GetParameters()
        match (GrpcMethod.getMethodType method) with
        | MethodType.Unary -> createUnaryMethod il returnType
        | _ -> failwith "TODO" 
        
    let private createMethod (typeBuilder:TypeBuilder) (baseMethod: MethodInfo) =
        let parameters = baseMethod.GetParameters()
        let parameterTypes = parameters |> Array.map (fun p -> p.ParameterType)
        
        // Define the method
        let methodBuilder = typeBuilder.DefineMethod(
            baseMethod.Name,
            MethodAttributes.Public ||| MethodAttributes.Virtual ||| MethodAttributes.ReuseSlot,
            baseMethod.ReturnType,
            parameterTypes)
        
        // Copy parameter names and attributes
        for i in 0..parameters.Length - 1 do        
            let param = parameters[i]
            let paramBuilder = methodBuilder.DefineParameter(i + 1, param.Attributes, param.Name)            
            if (param.HasDefaultValue) then            
                paramBuilder.SetConstant(param.DefaultValue)
                    
        let il = methodBuilder.GetILGenerator();
        createMethodBody il baseMethod
        
    let createConcreteServiceImplementation (serviceBaseType:Type) : Type =    
        if (serviceBaseType = null) then
            raise <| ArgumentNullException(nameof(serviceBaseType))        
        if not (serviceBaseType.IsAbstract) then
            raise <| ArgumentException($"Type {serviceBaseType.Name} must be abstract", nameof(serviceBaseType))
        
        let assemblyName = new AssemblyName($"Dynamic_{serviceBaseType.Name}_Assembly")
        let assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
        let moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule")
        let className = $"Concrete{System.IO.Path.GetRandomFileName()}{serviceBaseType.Name}".Replace(".", "")
        let typeBuilder = moduleBuilder.DefineType(
            className,
            TypeAttributes.Public ||| TypeAttributes.Class,
            serviceBaseType)
        
        let virtualMethods =
            serviceBaseType.GetMethods(BindingFlags.Public |||  BindingFlags.Instance)
            |> Array.filter (fun m -> m.IsVirtual && not(m.IsFinal) && m.DeclaringType = serviceBaseType)
            
        for method in virtualMethods do        
            createMethod typeBuilder method
                
        typeBuilder.CreateType()
        
    let unaryRegex = @"public\s+virtual.*Task<(?<ResponseType>\S+)>\s+(?<MethodName>\w+)\(\S+\s\w+,\s\S+\s\w+\)"    
    let methodSigRegex = @"public\s+virtual.+Task.*\(.*\)"
    let serviceBaseRegex = @"public\s+abstract\s+partial\s+class\s+(?<ServiceName>\S+)ServiceBase"
    let nsRegex = @"namespace\s(?<Namespace>\S+)\s+.*"
        
    let containsServiceBase (io:IOs) (csFile:string) =        
        let mutable found = false
        for l in io.File.ReadAllLines csFile do
            if (Regex.IsMatch(l, serviceBaseRegex)) then
              found <- true
        found
    
    let genService (csFile:string) =        
        let isDuplex (line:string) =
            line.Contains("IAsyncStreamReader") && line.Contains("IServerStreamWriter")
        let isServerStreaming (line:string) =
            line.Contains("IServerStreamWriter")
        let isClientStreaming (line:string) =
            line.Contains("IAsyncStreamReader")
        
        
        let mutable braceCounter = 0
        let mutable serviceName = ""
        let mutable canCopy = false
        let mutable lineNumber = 0        
        let mutable methodName = ""
        let mutable methodType = MethodType.Unary
        let mutable responseType = ""
        let mutable methodLine = ""
        
        let unaryTemplate =
            """            
            var resp = global::Tefin.Features.ServerHandler.RunUnary("{{SERVICE_NAME}}", "{{METHOD_NAME}}", request, context);
            return System.Threading.Tasks.Task.FromResult(({{RESPONSE_TYPE}}) resp);
            """
        let getTemplate (l:string) =
            if (isDuplex l) then
                """ throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, "")); """
            elif (isServerStreaming l) then
                """ throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, "")); """
            elif (isClientStreaming l) then
                """ throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, "")); """
            else
                let um = Regex.Match(l, unaryRegex)
                let methodName = um.Groups["MethodName"].Value                            
                let responseType = um.Groups["ResponseType"].Value
                unaryTemplate
                    .Replace("{{SERVICE_NAME}}", serviceName)
                    .Replace("{{METHOD_NAME}}", methodName)
                    .Replace("{{RESPONSE_TYPE}}", responseType)                             
        
        let lines = File.fileIO.ReadAllLines csFile        
        let sb = StringBuilder()        
        let mutable endPos = 0
        for l in lines do
            lineNumber <- lineNumber + 1
            if l.Contains('{') then
                braceCounter <- braceCounter + 1
            if l.Contains('}') then
                braceCounter <- braceCounter - 1
          
            if canCopy && (braceCounter = 0) then
                canCopy <- false
                ignore (sb.AppendLine "}")
                endPos <- lineNumber
                
            let m = Regex.Match(l, serviceBaseRegex)
            if  m.Success then
                braceCounter <- 0                
                let baseName = m.Groups["ServiceName"].Value
                serviceName <- "TefinImpl" + baseName + "Service"
                sb.AppendLine($"public class {serviceName} : {baseName}ServiceBase") |> ignore
                canCopy <- true
            else
                if (canCopy) then
                    let mr = Regex.Match(l, methodSigRegex)
                    if (mr.Success) then
                        methodLine <- l
                            
                    if l.Contains("StatusCode.Unimplemented") then                        
                        ignore (sb.AppendLine (getTemplate methodLine) )
                    else
                        ignore (sb.AppendLine l)

        (serviceName, sb.ToString(), endPos)
    
    let insertService (io:IOs) (csFile:string) =
        let lines = io.File.ReadAllLines csFile
        let serviceName, code, lineNumber = genService csFile
        
        match lines |> Array.tryFind (fun l -> l.Contains(serviceName)) with
        | None ->        
            let sb = StringBuilder()
            let mutable ln = 0
            for l in lines do
                ln <- ln + 1
                ignore(sb.AppendLine l)
                if ln = lineNumber then
                    sb.AppendLine().AppendLine(code).AppendLine()
                    |> ignore            
            io.File.WriteAllTextAsync csFile (sb.ToString())
        | Some _ -> Task.CompletedTask
        
            
    

