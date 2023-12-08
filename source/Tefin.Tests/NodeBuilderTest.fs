module Tefin.Tests.NodeBuilderTest
open System
open System.Collections.Generic
open System.Reflection
open System.Threading
open System.Threading.Tasks
open Tefin.Core
open Tefin.Core.Reflection
open Tefin.Tests.TestInputTypes
open Tefin.ViewModels.Explorer
open Tefin.ViewModels.Types
open Tefin.ViewModels.Types.TypeNodeBuilders
open Xunit

type Sample() =    
    member x.RunTest1(arg:Test1) =
        arg
    member x.RunTest1Array(arg:Test1 array) =
        arg
        
    member x.RunTest1Dict(arg:Dictionary<string, Test1>) =
        arg


let buildNode<'a> (methodName:string) =
    let struct(ok, instance) = buildType typeof<'a>       
    let mi = typeof<Sample>.GetMethod(methodName)
    let paramInfo = mi.GetParameters() |> Array.head
    let typeInfo = TypeInfo(paramInfo, instance)    
    let node = TypeNodeBuilder.Create(paramInfo.Name, paramInfo.ParameterType, typeInfo, Dictionary<string, int>(), instance, null);
    node.Init()
    node, typeInfo, instance

let validate (nodes:IExplorerItem seq) (typeProps : PropertyInfo array) (instance:obj) =
    let orderedNodes = nodes |> Seq.sortBy (fun c -> c.Title) |> Seq.toArray
    let orderedTypeProps = typeProps |> Array.sortBy (fun c -> c.Name)
    
    for i in [0 .. orderedTypeProps.Length - 1] do
        let childNode = orderedNodes[i] :?> TypeBaseNode
        let childProp = orderedTypeProps[i]
        Assert.Equal(childProp.Name, childNode.Title)
        let childValue = childProp.GetValue(instance)
        Assert.Equal(childValue, childNode.Value)
        
[<Fact>]
let ``Can build array object tree`` () =
    let (node, typeInfo, instance) = buildNode<Test1 array> "RunTest1Array"
    let typeProps = typeof<Test1>.GetProperties()
    
    Assert.Equal(typeInfo.Name, node.Title)
    Assert.Equal(1, node.Items.Count) //An array is created with 1 instance by default
    
    let arrayItemNodes = node.Items[0].Items
    let arrayItemInstance = (instance :?> Array).GetValue(0)
    validate arrayItemNodes typeProps arrayItemInstance

     
[<Fact>]
let ``Can build Dictionary object tree`` () =
    let (node, typeInfo, instance) = buildNode<Dictionary<string, Test1>> "RunTest1Dict"
    let typeProps = typeof<Test1>.GetProperties()
    
    Assert.Equal(typeInfo.Name, node.Title)
    
    //A dict is created with 1 "Item" node. This Item node will contain 2 nodes, Key and Value
    Assert.Equal(1, node.Items.Count) 
    Assert.Equal("Item[0]", node.Items[0].Title) 
        
    let keyNode = node.Items[0].Items[0] :?> TypeBaseNode
    let valueNode = node.Items[0].Items[1] :?> TypeBaseNode
    
    //1. Validate the Key
    Assert.Equal("Key", keyNode.Title)
    Assert.Equal(typeof<string>, keyNode.Type)
    Assert.Equal(box "", keyNode.Value)
    
    //2. Validate the Value
    Assert.Equal("Value", valueNode.Title)
    Assert.Equal(typeof<Test1>, valueNode.Type)
    validate valueNode.Items typeProps valueNode.Value
     
    
[<Fact>]
let ``Can build class object tree`` () =
    let (node, typeInfo, instance) = buildNode<Test1> "RunTest1"    
    let typeProps = typeof<Test1>.GetProperties()
    Assert.Equal(typeInfo.Name, node.Title)
    let typeProps = typeProps
    Assert.Equal(typeProps.Length, node.Items.Count)
    validate node.Items typeProps instance
    
    
    
[<Fact>]
let ``Can edit class object tree`` () =
    let (node, typeInfo, instance) = buildNode<Test1> "RunTest1"
    let target = Test1(
        BoolType = true,
        IntType = 42,
        DoubleType = 42.42       
        )
    
    let typeNodes = node.Items |> Seq.map (fun c -> c :?> TypeBaseNode) |> Seq.toArray
    
    let typeProps = typeof<Test1>.GetProperties()
    for p in typeProps do
        let propName = p.Name
        let targetPropValue = p.GetValue(target)
        let cn = typeNodes |> Array.find (fun c -> c.Title = propName)
        cn.Value <- targetPropValue
        let updatedValue = p.GetValue(instance)
        Assert.Equal(targetPropValue, updatedValue)
    
    
    
    
    
     
    
   
    
           