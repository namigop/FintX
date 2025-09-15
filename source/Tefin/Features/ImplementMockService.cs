using System.Reflection;
using System.Reflection.Emit;

using Grpc.Core;

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

using DynamicData.Tests;

using Northwind;

namespace Tefin.Features;

public class ImplementMockService {
    
}


//using Grpc.Core;

public class foo : ServerCallContext {
    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => throw new NotImplementedException();

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) => throw new NotImplementedException();

    protected override string MethodCore { get; }
    protected override string HostCore { get; }
    protected override string PeerCore { get; }
    protected override DateTime DeadlineCore { get; }
    protected override Metadata RequestHeadersCore { get; }
    protected override CancellationToken CancellationTokenCore { get; }
    protected override Metadata ResponseTrailersCore { get; }
    protected override Status StatusCore { get; set; }
    protected override WriteOptions? WriteOptionsCore { get; set; }
    protected override AuthContext AuthContextCore { get; }
}
public static class GenericServiceImplementationGenerator
{
    public static void Test() {
        var t = typeof(NorthwindService.NorthwindServiceBase);
        var concreteServiceType = CreateConcreteServiceImplementation(t);
        
        var inst = Activator.CreateInstance(concreteServiceType);
        var nw = inst as NorthwindService.NorthwindServiceBase;
        var order = nw.GetOrderById(new OrderRequest(), new foo());
todo

    }
    
    /// <summary>
    /// Creates a concrete implementation for any gRPC ServiceBase class at runtime
    /// </summary>
    /// <param name="serviceBaseType">The abstract ServiceBase class type</param>
    /// <param name="implementationName">Optional name for the generated class</param>
    /// <returns>Type of the generated concrete class</returns>
    public static Type CreateConcreteServiceImplementation(Type serviceBaseType, string implementationName = null)
    {
        if (serviceBaseType == null)
            throw new ArgumentNullException(nameof(serviceBaseType));
        
        if (!serviceBaseType.IsAbstract)
            throw new ArgumentException($"Type {serviceBaseType.Name} must be abstract", nameof(serviceBaseType));
        
        var assemblyName = new AssemblyName($"Dynamic_{serviceBaseType.Name}_Assembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        var className = implementationName ?? $"Concrete{serviceBaseType.Name}";
        var typeBuilder = moduleBuilder.DefineType(
            className,
            TypeAttributes.Public | TypeAttributes.Class,
            serviceBaseType);
        
        var virtualMethods = GetVirtualMethodsToImplement(serviceBaseType);
        foreach (var method in virtualMethods)
        {
            CreateGenericMethodOverride(typeBuilder, method);
        }
        
        return typeBuilder.CreateType();
    }
    
    /// <summary>
    /// Creates an instance of the concrete service implementation
    /// </summary>
    /// <typeparam name="TServiceBase">The ServiceBase type</typeparam>
    /// <param name="serviceBaseType">The abstract ServiceBase class type</param>
    /// <returns>Instance of the concrete implementation</returns>
    public static TServiceBase CreateServiceInstance<TServiceBase>(Type serviceBaseType) 
        where TServiceBase : class
    {
        var concreteType = CreateConcreteServiceImplementation(serviceBaseType);
        return (TServiceBase)Activator.CreateInstance(concreteType);
    }
    
    /// <summary>
    /// Creates an instance of the concrete service implementation
    /// </summary>
    /// <param name="serviceBaseType">The abstract ServiceBase class type</param>
    /// <returns>Instance of the concrete implementation</returns>
    public static object CreateServiceInstance(Type serviceBaseType)
    {
        var concreteType = CreateConcreteServiceImplementation(serviceBaseType);
        return Activator.CreateInstance(concreteType);
    }
    
    private static MethodInfo[] GetVirtualMethodsToImplement(Type serviceBaseType)
    {
        return serviceBaseType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.IsVirtual && !m.IsFinal && m.DeclaringType == serviceBaseType)
            .ToArray();
    }
    
    private static void CreateGenericMethodOverride(TypeBuilder typeBuilder, MethodInfo baseMethod)
    {
        // Get method parameters
        var parameters = baseMethod.GetParameters();
        var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
        
        // Define the method
        var methodBuilder = typeBuilder.DefineMethod(
            baseMethod.Name,
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.ReuseSlot,
            baseMethod.ReturnType,
            parameterTypes);
        
        // Copy parameter names and attributes
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var paramBuilder = methodBuilder.DefineParameter(i + 1, param.Attributes, param.Name);
            
            // Copy default values if any
            if (param.HasDefaultValue)
            {
                paramBuilder.SetConstant(param.DefaultValue);
            }
        }
        
        // Generate the method body
        var il = methodBuilder.GetILGenerator();
        GenerateGenericMethodBody(il, baseMethod);
    }
    
    private static void GenerateGenericMethodBody(ILGenerator il, MethodInfo method)
    {
        var returnType = method.ReturnType;
        var parameters = method.GetParameters();
        
        // Analyze the method signature to determine the gRPC pattern
        var grpcPattern = DetermineGrpcPattern(method, parameters);
        
        switch (grpcPattern)
        {
            case GrpcMethodPattern.Unary:
                GenerateUnaryMethodBody(il, returnType);
                break;
            case GrpcMethodPattern.ServerStreaming:
                GenerateServerStreamingMethodBody(il);
                break;
            case GrpcMethodPattern.ClientStreaming:
                GenerateClientStreamingMethodBody(il, returnType);
                break;
            case GrpcMethodPattern.BidirectionalStreaming:
                GenerateBidirectionalStreamingMethodBody(il);
                break;
            case GrpcMethodPattern.Unknown:
            default:
                GenerateDefaultMethodBody(il, returnType);
                break;
        }
    }
    
    private static GrpcMethodPattern DetermineGrpcPattern(MethodInfo method, ParameterInfo[] parameters)
    {
        // Check for streaming parameters
        bool hasRequestStream = parameters.Any(p => IsAsyncStreamReader(p.ParameterType));
        bool hasResponseStream = parameters.Any(p => IsServerStreamWriter(p.ParameterType));
        
        // Determine pattern based on streaming parameters
        if (hasRequestStream && hasResponseStream)
            return GrpcMethodPattern.BidirectionalStreaming;
        else if (hasRequestStream)
            return GrpcMethodPattern.ClientStreaming;
        else if (hasResponseStream)
            return GrpcMethodPattern.ServerStreaming;
        else if (parameters.Length >= 1 && parameters.Last().ParameterType == typeof(ServerCallContext))
            return GrpcMethodPattern.Unary;
        else
            return GrpcMethodPattern.Unknown;
    }
    
    private static bool IsAsyncStreamReader(Type type)
    {
        return type.IsGenericType && 
               type.GetGenericTypeDefinition().Name.StartsWith("IAsyncStreamReader");
    }
    
    private static bool IsServerStreamWriter(Type type)
    {
        return type.IsGenericType && 
               type.GetGenericTypeDefinition().Name.StartsWith("IServerStreamWriter");
    }
    
    private static void GenerateUnaryMethodBody(ILGenerator il, Type returnType)
    {
        // For unary calls: Task<TResponse>
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var responseType = returnType.GetGenericArguments()[0];
            GenerateDefaultResponseObject(il, responseType);
            
            var fromResultMethod = typeof(Task).GetMethod("FromResult").MakeGenericMethod(responseType);
            il.Emit(OpCodes.Call, fromResultMethod);
        }
        else
        {
            GenerateDefaultMethodBody(il, returnType);
        }
        
        il.Emit(OpCodes.Ret);
    }
    
    private static void GenerateServerStreamingMethodBody(ILGenerator il)
    {
        // For server streaming: Task (no return value, writes to stream)
        var completedTaskProperty = typeof(Task).GetProperty("CompletedTask");
        il.Emit(OpCodes.Call, completedTaskProperty.GetMethod);
        il.Emit(OpCodes.Ret);
    }
    
    private static void GenerateClientStreamingMethodBody(ILGenerator il, Type returnType)
    {
        // For client streaming: Task<TResponse>
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var responseType = returnType.GetGenericArguments()[0];
            GenerateDefaultResponseObject(il, responseType);
            
            var fromResultMethod = typeof(Task).GetMethod("FromResult").MakeGenericMethod(responseType);
            il.Emit(OpCodes.Call, fromResultMethod);
        }
        else
        {
            var completedTaskProperty = typeof(Task).GetProperty("CompletedTask");
            il.Emit(OpCodes.Call, completedTaskProperty.GetMethod);
        }
        
        il.Emit(OpCodes.Ret);
    }
    
    private static void GenerateBidirectionalStreamingMethodBody(ILGenerator il)
    {
        // For bidirectional streaming: Task (no return value, reads and writes streams)
        var completedTaskProperty = typeof(Task).GetProperty("CompletedTask");
        il.Emit(OpCodes.Call, completedTaskProperty.GetMethod);
        il.Emit(OpCodes.Ret);
    }
    
    private static void GenerateDefaultResponseObject(ILGenerator il, Type responseType)
    {
        try
        {
            // Try to create a default instance of the response type
            var defaultConstructor = responseType.GetConstructor(Type.EmptyTypes);
            
            if (defaultConstructor != null)
            {
                // Create new instance using parameterless constructor
                il.Emit(OpCodes.Newobj, defaultConstructor);
            }
            else
            {
                // Try to use Activator.CreateInstance for types without parameterless constructor
                il.Emit(OpCodes.Ldtoken, responseType);
                il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                il.Emit(OpCodes.Call, typeof(Activator).GetMethod("CreateInstance", new[] { typeof(Type) }));
                il.Emit(OpCodes.Castclass, responseType);
            }
        }
        catch
        {
            // If all else fails, emit null
            il.Emit(OpCodes.Ldnull);
        }
    }
    
    private static void GenerateDefaultMethodBody(ILGenerator il, Type returnType)
    {
        if (returnType == typeof(Task))
        {
            // Return Task.CompletedTask
            var completedTaskProperty = typeof(Task).GetProperty("CompletedTask");
            il.Emit(OpCodes.Call, completedTaskProperty.GetMethod);
        }
        else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            // Return Task.FromResult(default(T))
            var genericArg = returnType.GetGenericArguments()[0];
            GenerateDefaultValue(il, genericArg);
            
            var fromResultMethod = typeof(Task).GetMethod("FromResult").MakeGenericMethod(genericArg);
            il.Emit(OpCodes.Call, fromResultMethod);
        }
        else
        {
            // For non-Task return types, return default value
            GenerateDefaultValue(il, returnType);
        }
        
        il.Emit(OpCodes.Ret);
    }
    
    private static void GenerateDefaultValue(ILGenerator il, Type type)
    {
        if (type.IsValueType)
        {
            var defaultValue = il.DeclareLocal(type);
            il.Emit(OpCodes.Ldloca_S, defaultValue);
            il.Emit(OpCodes.Initobj, type);
            il.Emit(OpCodes.Ldloc, defaultValue);
        }
        else
        {
            il.Emit(OpCodes.Ldnull);
        }
    }
    
    private enum GrpcMethodPattern
    {
        Unknown,
        Unary,
        ServerStreaming,
        ClientStreaming,
        BidirectionalStreaming
    }
}

// Extension methods for easier usage
public static class ServiceImplementationExtensions
{
    /// <summary>
    /// Creates a concrete implementation for any gRPC ServiceBase class
    /// </summary>
    /// <typeparam name="TServiceBase">The ServiceBase type</typeparam>
    /// <returns>Instance of the concrete implementation</returns>
    public static TServiceBase CreateConcreteImplementation<TServiceBase>() 
        where TServiceBase : class
    {
        var serviceBaseType = typeof(TServiceBase);
        return GenericServiceImplementationGenerator.CreateServiceInstance<TServiceBase>(serviceBaseType);
    }
    
    /// <summary>
    /// Creates a concrete implementation for a gRPC ServiceBase class by type
    /// </summary>
    /// <param name="serviceBaseType">The ServiceBase class type</param>
    /// <returns>Instance of the concrete implementation</returns>
    public static object CreateConcreteImplementation(this Type serviceBaseType)
    {
        return GenericServiceImplementationGenerator.CreateServiceInstance(serviceBaseType);
    }
}

// Usage examples
public static class UsageExamples
{
    // Example 1: When you have the type at compile time
    public static void ExampleWithKnownType()
    {
        // Using extension method
        var service = ServiceImplementationExtensions.CreateConcreteImplementation<NorthwindService.NorthwindServiceBase>();
        
        // Or using the generator directly
        var service2 = GenericServiceImplementationGenerator.CreateServiceInstance<NorthwindService.NorthwindServiceBase>(typeof(NorthwindService.NorthwindServiceBase));
        
        Console.WriteLine($"Created service of type: {service.GetType().Name}");
    }
    
    // Example 2: When you only have the type at runtime (e.g., from assembly scanning)
    public static void ExampleWithRuntimeType(Assembly grpcAssembly)
    {
        // Scan assembly for ServiceBase classes
        var serviceBaseTypes = grpcAssembly.GetTypes()
            .Where(t => t.IsAbstract && t.Name.EndsWith("ServiceBase"))
            .ToList();
        
        foreach (var serviceBaseType in serviceBaseTypes)
        {
            Console.WriteLine($"Found ServiceBase: {serviceBaseType.Name}");
            
            // Create concrete implementation
            var serviceInstance = serviceBaseType.CreateConcreteImplementation();
            
            Console.WriteLine($"Created concrete implementation: {serviceInstance.GetType().Name}");
            
            // You can now register this with your DI container or gRPC server
            // Example with ASP.NET Core DI:
            // services.AddSingleton(serviceBaseType, serviceInstance);
        }
    }
    
    // Example 3: Creating multiple implementations with custom names
    public static void ExampleWithCustomNames()
    {
        var serviceBaseType = typeof(NorthwindService.NorthwindServiceBase);
        
        // Create multiple implementations with different names
        var mockImplementationType = GenericServiceImplementationGenerator
            .CreateConcreteServiceImplementation(serviceBaseType, "MockNorthwindService");
        
        var stubImplementationType = GenericServiceImplementationGenerator
            .CreateConcreteServiceImplementation(serviceBaseType, "StubNorthwindService");
        
        var mockInstance = Activator.CreateInstance(mockImplementationType);
        var stubInstance = Activator.CreateInstance(stubImplementationType);
        
        Console.WriteLine($"Mock: {mockInstance.GetType().Name}");
        Console.WriteLine($"Stub: {stubInstance.GetType().Name}");
    }
    
    // Example 4: Integration with dependency injection
    public static void RegisterDynamicServices(IServiceCollection services, Assembly grpcAssembly)
    {
        var serviceBaseTypes = grpcAssembly.GetTypes()
            .Where(t => t.IsAbstract && 
                       t.Name.EndsWith("ServiceBase") && 
                       t.GetCustomAttribute<global::Grpc.Core.BindServiceMethodAttribute>() != null)
            .ToList();
        
        foreach (var serviceBaseType in serviceBaseTypes)
        {
            var concreteType = GenericServiceImplementationGenerator
                .CreateConcreteServiceImplementation(serviceBaseType);
            
            services.AddScoped(serviceBaseType, provider => Activator.CreateInstance(concreteType));
        }
    }
}

// Alternative implementation using Castle DynamicProxy for comparison
/*
using Castle.DynamicProxy;

public class GenericGrpcServiceInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        var method = invocation.Method;
        var returnType = method.ReturnType;
        var parameters = method.GetParameters();
        
        // Analyze method signature to determine gRPC pattern
        bool hasRequestStream = parameters.Any(p => IsAsyncStreamReader(p.ParameterType));
        bool hasResponseStream = parameters.Any(p => IsServerStreamWriter(p.ParameterType));
        
        if (returnType == typeof(Task))
        {
            // Streaming methods that return Task
            invocation.ReturnValue = Task.CompletedTask;
        }
        else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            // Unary or client streaming methods that return Task<T>
            var responseType = returnType.GetGenericArguments()[0];
            var defaultResponse = CreateDefaultInstance(responseType);
            
            var fromResultMethod = typeof(Task).GetMethod("FromResult").MakeGenericMethod(responseType);
            invocation.ReturnValue = fromResultMethod.Invoke(null, new[] { defaultResponse });
        }
        else
        {
            // Non-async methods (rare in gRPC)
            invocation.ReturnValue = returnType.IsValueType ? 
                Activator.CreateInstance(returnType) : null;
        }
    }
    
    private object CreateDefaultInstance(Type type)
    {
        try
        {
            return Activator.CreateInstance(type);
        }
        catch
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
    
    private bool IsAsyncStreamReader(Type type)
    {
        return type.IsGenericType && 
               type.GetGenericTypeDefinition().Name.StartsWith("IAsyncStreamReader");
    }
    
    private bool IsServerStreamWriter(Type type)
    {
        return type.IsGenericType && 
               type.GetGenericTypeDefinition().Name.StartsWith("IServerStreamWriter");
    }
}

public static class CastleProxyGenericExample
{
    public static TServiceBase CreateServiceWithCastle<TServiceBase>() 
        where TServiceBase : class
    {
        var proxyGenerator = new ProxyGenerator();
        var interceptor = new GenericGrpcServiceInterceptor();
        
        return proxyGenerator.CreateClassProxy<TServiceBase>(interceptor);
    }
}
*/