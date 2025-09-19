namespace Tefin.Grpc

open System.Reflection
open System.Reflection.Emit
open System.Threading.Tasks
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
    

