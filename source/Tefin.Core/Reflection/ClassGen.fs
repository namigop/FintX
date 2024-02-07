namespace Tefin.Core.Reflection

open System
open System.Reflection
open System.Reflection.Emit

type PropInfo =
  { IsMethod: bool
    Name: string
    Type: Type }

module ClassGen =

  let private getTypeBuilder (assemblyName: string) (moduleName: string) (className: string) =
    let an = AssemblyName assemblyName

    let assemblyBuilder =
      AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run)

    let moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName)

    let tb =
      moduleBuilder.DefineType(
        className,
        TypeAttributes.Public
        ||| TypeAttributes.Class
        ||| TypeAttributes.AutoClass
        ||| TypeAttributes.AnsiClass
        ||| TypeAttributes.BeforeFieldInit
        ||| TypeAttributes.AutoLayout,
        null
      )

    tb

  let private createGetMethod (myTypeBld: TypeBuilder) (methodName: string) (returnType: Type) =
    let methodBuilder =
      myTypeBld.DefineMethod(methodName, MethodAttributes.Public, returnType, null)

    let ILout = methodBuilder.GetILGenerator()
    ILout.Emit(OpCodes.Ret)

  let private createProperty (tb: TypeBuilder) (propertyName: string) (propertyType: Type) =
    if not (typeof<unit> = propertyType) then
      let fieldBuilder =
        tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private)

      let propertyBuilder =
        tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null)

      let getPropMethodBuilder =
        tb.DefineMethod(
          "get_" + propertyName,
          MethodAttributes.Public
          ||| MethodAttributes.SpecialName
          ||| MethodAttributes.HideBySig,
          propertyType,
          Type.EmptyTypes
        )

      let getIl = getPropMethodBuilder.GetILGenerator()

      getIl.Emit(OpCodes.Ldarg_0)
      getIl.Emit(OpCodes.Ldfld, fieldBuilder)
      getIl.Emit(OpCodes.Ret)

      let setPropMethodBuilder =
        tb.DefineMethod(
          "set_" + propertyName,
          MethodAttributes.Public
          ||| MethodAttributes.SpecialName
          ||| MethodAttributes.HideBySig,
          null,
          [| propertyType |]
        )

      let setIl = setPropMethodBuilder.GetILGenerator()
      let modifyProperty = setIl.DefineLabel()
      let exitSet = setIl.DefineLabel()
      setIl.MarkLabel(modifyProperty)
      setIl.Emit(OpCodes.Ldarg_0)
      setIl.Emit(OpCodes.Ldarg_1)
      setIl.Emit(OpCodes.Stfld, fieldBuilder)

      setIl.Emit(OpCodes.Nop)
      setIl.MarkLabel(exitSet)
      setIl.Emit(OpCodes.Ret)

      propertyBuilder.SetGetMethod(getPropMethodBuilder)
      propertyBuilder.SetSetMethod(setPropMethodBuilder)

  let create (assemblyName: string) (moduleName: string) (className: string) (targetProps: PropInfo array) =
    let tb = getTypeBuilder assemblyName moduleName className

    let _ =
      tb.DefineDefaultConstructor(
        MethodAttributes.Public
        ||| MethodAttributes.SpecialName
        ||| MethodAttributes.RTSpecialName
      )

    let targetMethods, targetProperties =
      targetProps |> Array.partition (fun t -> t.IsMethod)

    for p in targetProperties do
      createProperty tb p.Name p.Type

    for p in targetMethods do
      createGetMethod tb p.Name p.Type

    tb.CreateType()
