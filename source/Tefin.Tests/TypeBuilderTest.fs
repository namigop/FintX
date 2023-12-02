module Tefin.Tests.TypeBuilderTest

open Tefin.Tests.TestInputTypes
open Tefin.ViewModels.Types
open Tefin.ViewModels.Types.TypeNodeBuilders
open Xunit

[<Fact>]
let ``Can build instance tree``() =
     let instance = Test2()
     let name = "test1"
     let typ = typeof<Test2>
     let typeInfo = typ.GetProperty("Test1Type") |> TypeInfo
     let node = TypeNodeBuilder.Create(name, instance)
     node.Init()
     Assert.Equal(1, node.Items.Count)
    