namespace rec Tefin.Core.Reflection

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Reflection
open System.Threading
open Bogus
open Microsoft.FSharp.Core
open Tefin.Core.Interop

module Faker =
  type Fk =
    {
      Name:string
      NameParts : string array
      Type:Type
      Get: unit -> obj
    }
    
  let fakerInfo =
      let temp = Dictionary<string, Fk>()
      let f = Faker()    
      let props = f.GetType().GetProperties()
      let excluded = [| "WithHost"; "ToString"; "Equals"; "GetType"; "GetHashCode"; "ToUpperInvariant"; "ReplaceLineEndings" |]
      for prop in props do
        let propInst = prop.GetValue f
        let mis = prop.PropertyType.GetMethods()
                  |> Array.filter (fun m -> not m.IsSpecialName)
                  |> Array.filter (fun m -> m.Name.Length > 2)
                  |> Array.filter (fun m ->  excluded |> Array.contains m.Name |> not )
        for mi in mis do
          let methodParams = mi.GetParameters()
          if (methodParams.Length = 0) then
            temp.[mi.Name] <-
              { Name = mi.Name.ToLowerInvariant()
                NameParts = Tefin.Core.Utils.splitWord mi.Name |> Array.map _.ToLowerInvariant()
                Type = mi.ReturnType
                Get = fun () -> mi.Invoke(propInst, null) }
          
          let optionalParams = methodParams |> Array.filter (fun p -> p.IsOptional && p.HasDefaultValue)
          if optionalParams.Length = methodParams.Length then
            temp.[mi.Name] <-
              { Name = mi.Name.ToLowerInvariant()
                NameParts = Tefin.Core.Utils.splitWord mi.Name |> Array.map _.ToLowerInvariant()
                Type = mi.ReturnType
                Get = fun () ->
                  let args = optionalParams |> Array.map _.DefaultValue
                  mi.Invoke(propInst, args) }
      temp
      
      
  let private specialRules (name2:string) (type2: Type) : struct (bool * obj) =
    let nameParts = (Tefin.Core.Utils.splitWord name2) |> Array.map _.ToLowerInvariant()   
    let matchCounts = fakerInfo.Values |> Seq.map (fun v ->      
      if type2 = v.Type then
        let mutable count = 0.0
        for n in nameParts do
          let found = v.NameParts.Contains n
          if found then
            count <- count + 1.0
        v, (count / (Convert.ToDouble nameParts.Length))
      else
        v, 0
      )
    
    let name = name2.ToLowerInvariant()
    let f = Faker()
    
    if name.Contains("postal") && type2 = typeof<string> then
      struct (true, f.Address.ZipCode())
    elif name.Contains("address") && type2 = typeof<string> then
      struct (true, f.Address.FullAddress())
    elif name.Contains("location") && type2 = typeof<string> then
      struct (true, f.Address.FullAddress())
    elif name.Contains("weight") && type2 = typeof<string> then
      struct (true, "kg")  
    elif name.Contains("image") && (name.Contains("url") || name.Contains("uri")) && type2 = typeof<string> then
      struct (true, f.Image.PlaceImgUrl())
    elif name.Contains("sku") && type2 = typeof<string> then
      let p1 = f.Random.AlphaNumeric(3).ToUpperInvariant()
      let p2 = f.Random.Digits(3,1, 9) |> fun c -> String.Join("", c)
      let p3 = f.Random.AlphaNumeric(2).ToUpperInvariant()      
      struct (true, $"{p1}-{p2}-{p3}")      
    elif name = "id" then
      struct (true, f.Random.AlphaNumeric(6).ToUpperInvariant())
    elif name.Contains "account" && name.Contains "number" && type2 = typeof<string> then
      let p1 = f.Random.Digits(4,1, 9) |> fun c -> String.Join("", c)
      let p2 = f.Random.Digits(4,1, 9) |> fun c -> String.Join("", c)
      let p3 = f.Random.Digits(4,1, 9) |> fun c -> String.Join("", c)
      struct (true, $"{p1}-{p2}-{p3}")
    elif name.Contains "transaction" && (name.EndsWith "number" || name.EndsWith "id") && type2 = typeof<string> then
      let p1 = f.Random.Digits(12,1, 9) |> fun c -> String.Join("", c)
      struct (true, p1)
    elif name.Contains "currency" && type2 = typeof<string>  then      
      struct (true, f.Finance.Currency().Code)
    elif name.EndsWith("id") &&
         (name.Contains("employee") ||
          name.Contains("customer") ||
          name.Contains("product") ||
          name.Contains("order") ) && type2 = typeof<string> then
      let r = f.Random.AlphaNumeric(6).ToUpperInvariant()
      struct (true, r)      
    else
      let cur = matchCounts |> Seq.filter (fun (c,_) -> c.Name = "currency" ) |> Seq.toArray
      let (highestMatch, score) = matchCounts |> Seq.sortByDescending (fun (fk, count) -> count) |> Seq.head
      if (score >= 0.5) then
        let value = highestMatch.Get()
        //Console.WriteLine $"Match Score = {score} - {name} vs {highestMatch.Name} -> {value}"
        struct (true, value)
      else
        struct (false, null)
        
  let private calcScore (names:string array) (targets:string array) =
        let mutable score = 0.0
        for name in names do
          for i in targets do
            score <- score +  Tefin.Core.Utils.calculateSimilarity name i        
        score / (Convert.ToDouble names.Length)
        
  
  let fakerMap (name:string) (targetType2:Type) =  
    let targetType =
      if TypeHelper.isNullable(targetType2) then
        Nullable.GetUnderlyingType(targetType2)
      else
        targetType2
    
    let ok, v = fakerInfo.TryGetValue (name.ToLowerInvariant())
    if (ok && v.Type = targetType) then
      struct(true, v.Get())  
    else          
      let scores = Dictionary<float, Fk>()
      for i in fakerInfo do            
        if (i.Value.Type = targetType) then            
          let score = calcScore (Tefin.Core.Utils.splitWord name) i.Value.NameParts              
          if score > 0.7 then                
            //let foo = i.Value.Get()
            scores[score] <- i.Value 
      
      let matched = scores.Count > 0
      if matched then
        let inst = scores |> Seq.sortByDescending _.Key |> Seq.head            
        let defaultValue = inst.Value.Get()
        //Console.WriteLine $"Score = {inst.Key} - {name} vs {inst.Value.Name} -> {defaultValue}"
        struct(matched, defaultValue)
      else
        struct (false, null)
   
        
  let getDefault (name:string) (type2: Type) (createInstance: bool) (parentInstance: obj option) (depth: int) =
    if (SystemType.isSystemType type2) then
      let struct (ok, v) = (specialRules name type2)
      if ok then struct (ok, v)
      else fakerMap name type2
    else
        struct (false, null)
   

module TypeBuilder =
  let private handlers =     
    ResizeArray(
      [| Faker.getDefault
         SystemType.getDefault
         ArrayType.getDefault
         DictionaryType.getDefault
         GenericListType.getDefault
         ClassType.getDefault |]
    )

  let register handler = handlers.Insert(0, handler) //side-effect
    
  let getDefault (name:string) (type2: Type) (createInstance: bool) (parentInstance: obj option) (depth: int) =
    let result =
      [ for h in handlers -> h ]
      |> Seq.fold
        (fun state handleFunc ->
          let struct (handled: bool, _) = state
          if handled then
            state
          else
            let ret = handleFunc name type2 createInstance parentInstance depth
            ret)
        (struct (false, Unchecked.defaultof<obj>))
    result
    
module SystemType =

  let private info =    
    let markerToken = (new CancellationTokenSource()).Token
    let temp = Dictionary<Type, (unit -> obj) * string>()
    temp.Add(typeof<int>, ((fun () -> Random.Shared.Next(0,100) ), "int"))
    temp.Add(typeof<int16>, ((fun () -> Random.Shared.Next(0,100)), "int16"))
    temp.Add(typeof<int64>, ((fun () -> Random.Shared.Next(0,100)), "long"))
    temp.Add(typeof<decimal>, ((fun () -> Random.Shared.Next(0,100)), "dec"))
    temp.Add(typeof<Double>, ((fun () -> Random.Shared.Next(0,100)), "float"))
    temp.Add(typeof<Single>, ((fun () -> Random.Shared.Next(0,100)), "float32"))
    temp.Add(typeof<uint>, ((fun () -> Random.Shared.Next(1,100)), "uint"))
    temp.Add(typeof<uint16>, ((fun () -> Random.Shared.Next(1,100)), "uint16"))
    temp.Add(typeof<uint64>, ((fun () -> Random.Shared.Next(1,100)), "ulong"))
    temp.Add(typeof<bool>, ((fun () -> true), "bool"))
    temp.Add(typeof<DateTime>, ((fun () -> DateTime.Now.AddDays 1), "dateTime"))
    temp.Add(typeof<DateTimeOffset>, ((fun () -> DateTimeOffset.Now.AddDays 1), "dtOffset"))
    temp.Add(typeof<Guid>, ((fun () -> Guid.NewGuid()), "guid"))
    temp.Add(typeof<TimeSpan>, ((fun () -> TimeSpan.FromSeconds 1L), "timespan"))
    //temp.Add(typeof<CancellationToken>, ((fun () -> CancellationToken.None), "token"))
    temp.Add(typeof<CancellationToken>, ((fun () -> markerToken), "token"))
    temp.Add(typeof<string>, ((fun () -> Path.GetRandomFileName()), "string"))
    temp.Add(typeof<char>, ((fun () -> 'c'), "char"))
    temp.Add(typeof<byte>, ((fun () -> byte 0), "byte"))
    temp.Add(typeof<Uri>, ((fun () -> Uri("http://localhost:8080/")), "uri"))

    temp.Add(typeof<Nullable<int>>, ((fun () -> Random.Shared.Next(0,100)), "int?"))
    temp.Add(typeof<Nullable<int16>>, ((fun () -> Random.Shared.Next(0,100)), "int16?"))
    temp.Add(typeof<Nullable<int64>>, ((fun () -> Random.Shared.Next(0,100)), "long?"))
    temp.Add(typeof<Nullable<decimal>>, ((fun () -> Random.Shared.Next(0,100)), "dec?"))
    temp.Add(typeof<Nullable<Double>>, ((fun () -> Random.Shared.Next(0,100)), "float?"))
    temp.Add(typeof<Nullable<Single>>, ((fun () -> Random.Shared.Next(0,100)), "float32?"))
    temp.Add(typeof<Nullable<uint>>, ((fun () -> Random.Shared.Next(0,100)), "uint?"))
    temp.Add(typeof<Nullable<uint16>>, ((fun () -> Random.Shared.Next(0,100)), "uint16?"))
    temp.Add(typeof<Nullable<uint64>>, ((fun () -> Random.Shared.Next(0,100)), "ulong?"))
    temp.Add(typeof<Nullable<bool>>, ((fun () -> true), "bool?"))
    temp.Add(typeof<Nullable<DateTime>>, ((fun () -> DateTime.Now.AddDays 1), "dateTime?"))
    //temp.Add(typeof<Nullable<DateTime>>, ((fun () -> null), "dateTime?"))
    temp.Add(typeof<Nullable<DateTimeOffset>>, ((fun () -> DateTimeOffset.Now.AddDays 1), "dtOffset?"))
    temp.Add(typeof<Nullable<TimeSpan>>, ((fun () -> TimeSpan.FromSeconds 1L), "timespan?"))
    temp.Add(typeof<Nullable<char>>, ((fun () -> 'c'), "char?"))
    temp.Add(typeof<Google.Protobuf.WellKnownTypes.Timestamp>, ((fun () -> Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)), "timestamp"))   
    temp

  
  let getDisplayName (thisType: Type) =
    let ok, (_, display) = info.TryGetValue(thisType)
    if ok then display else "not a system type"
    

  let getTypes() = info.Keys
  let getTypesForDisplay() =
    info
    |> Seq.map (fun kv -> kv.Value)
    |> Seq.map (fun (_, display) -> display)
    |> Seq.toArray
    //|> Seq.append
    
  let getDisplayType  =
    fun (actualTypeFullName:string) ->                    
        let displayTypes = getTypesForDisplay()
        let actualTypes = getTypes() |> Seq.map (fun t -> t.FullName) |> Seq.toArray
        let index = Array.IndexOf(actualTypes, actualTypeFullName)
        if index >= 0 then (true, displayTypes.[index]) else (false, "not a system type")
    
  let getActualType  =
    fun (displayType:string) ->
        let displayTypes = getTypesForDisplay()
        let actualTypes = getTypes() |> Seq.map (fun t -> t.FullName) |> Seq.toArray
        let index = Array.IndexOf(displayTypes, displayType)
        if index >= 0 then (true, actualTypes.[index]) else (false,"not a system type")
           
  let isSystemType (thisType: Type) =
    if thisType = typeof<Google.Protobuf.WellKnownTypes.Timestamp> then
      false
    else  
      let ok, _ = info.TryGetValue(thisType)
      ok

  let getDefault (name:string) (thisType: Type) (createInstance: bool) (parentInstance: obj option) depth =
    if thisType.IsEnum then
      let enumVals = Enum.GetValues(thisType)
      let v = enumVals.GetValue(Random.Shared.Next(0, enumVals.Length))
      struct (true, v)
    elif info.ContainsKey(thisType) then
      let gen, _ = info[thisType]
      struct (true, gen ())
    else
      struct (false, TypeHelper.getDefault thisType)

module ArrayType =
  let getDefault (name:string) (thisType: Type) (createInstance: bool) (parentInstance: obj option) depth =
    if thisType.IsArray then
      let elementType = thisType.GetElementType()
      let instance = Array.CreateInstance(elementType, 1)

      let struct (ok, element) =
        TypeBuilder.getDefault "" elementType createInstance None depth

      if ok then
        instance.SetValue(element, 0)

      struct (true, instance)
    else
      struct (false, TypeHelper.getDefault thisType)

module GenericListType =
  let getDefault (name:string) (thisType: Type) (createInstance: bool) (parentInstance: obj option) depth =
    if (TypeHelper.isGenericListType thisType) && createInstance then
      let instance2 =
        let instance = Activator.CreateInstance(thisType)

        if not (instance = null) then
          match TypeHelper.getListItemType thisType with
          | Some elementType ->
            let struct (ok, elementInstance) =
              TypeBuilder.getDefault "" elementType true None depth

            if ok then
              let addMethod = thisType.GetMethod("Add", [| elementType |])
              addMethod.Invoke(instance, [| elementInstance |]) |> ignore
              instance
            else
              instance
          | None -> instance
        else
          instance

      struct (true, instance2)
    else
      struct (false, TypeHelper.getDefault thisType)

module ClassType =
  let private assignWritableProps (prop: PropertyInfo) (instance: obj) (isIndexParams: bool) depth =
    if isIndexParams then
      () //ignore indexed parameters like List[0]
    else
      let mutable objV = Unchecked.defaultof<obj>

      try
        let struct (_, newValue) = TypeBuilder.getDefault prop.Name prop.PropertyType true None depth
        objV <- newValue
        prop.SetValue(instance, newValue)
      with exc ->
        let msg =
          $"Unable to assign to {instance.GetType().Name}.{prop.Name} ({prop.PropertyType.Name}) = {objV}{Environment.NewLine}"

        System.Diagnostics.Debug.WriteLine(msg + exc.ToString())

  let private fillReadonlyProps
    (prop: PropertyInfo)
    (instance: obj)
    (isIndexParams: bool)
    (indexParams: ParameterInfo array)
    depth
    =
    if
      (isIndexParams
       && (indexParams[0].ParameterType = typeof<int>
           || indexParams[0].ParameterType = typeof<int64>))
    then
      let count =
        let countInfo = instance.GetType().GetProperty("Count")

        if not (countInfo = null) then
          Convert.ToInt32(countInfo.GetValue(instance))
        else
          let lengthInfo = instance.GetType().GetProperty("Length")

          if not (lengthInfo = null) then
            Convert.ToInt32(lengthInfo.GetValue(instance))
          else
            0

      if (count > 0) then
        let a = prop.GetValue(instance, [| count - 1 |])
        let _ = TypeBuilder.getDefault prop.Name prop.PropertyType false (Some(a)) depth
        ()
    else
      let currentInstance = prop.GetValue(instance)
      let _ = TypeBuilder.getDefault prop.Name prop.PropertyType true (Some currentInstance) depth
      ()

  let fill (instance: obj) (depth: int) =
    if (depth > 4) then
      instance
    else
      let editableProps =
        instance.GetType().GetProperties(BindingFlags.Instance ||| BindingFlags.Public)
        |> Array.filter (fun p -> p.CanRead)

      for prop in editableProps do
        let indexParams = prop.GetIndexParameters()
        let isIndexParams = not (indexParams = null) && indexParams.Length > 0

        //Console.WriteLine $"Processing {prop.Name}"
        if prop.CanWrite then
          assignWritableProps prop instance isIndexParams depth
        elif prop.CanRead then
          fillReadonlyProps prop instance isIndexParams indexParams depth
        else
          ()

      instance

  let getDefault (name:string) (thisType: Type) (createInstance: bool) (parentInstance: obj option) depth =
    if thisType.IsClass && not thisType.IsAbstract then
      if createInstance then
        let constructor = thisType.GetConstructor(Type.EmptyTypes)
        let mutable instance = Unchecked.defaultof<obj>

        if not (constructor = null) then
          instance <- Activator.CreateInstance(thisType)
          System.Diagnostics.Debug.WriteLine($"Calling constructor of {thisType.FullName}")
          let _ = fill instance (depth + 1)
          ()

        struct (true, instance)
      else
        struct (true, TypeHelper.getDefault thisType)
    else
      struct (false, TypeHelper.getDefault thisType)

module DictionaryType =
  let getDefault (name:string) (thisType: Type) (createInstance: bool) (parentInstanceOpt: obj option) depth =
    if thisType.IsGenericType && (TypeHelper.isDictionaryType thisType) then
      let keyValTypes = thisType.GetGenericArguments()

      let parentInstance =
        if (createInstance || parentInstanceOpt.IsNone) then
          Activator.CreateInstance(thisType)
        else
          parentInstanceOpt.Value

      let struct (_, keyInstance) = TypeBuilder.getDefault "" keyValTypes[0] true None depth
      let struct (_, valInstance) = TypeBuilder.getDefault "" keyValTypes[1] true None depth

      let add =
        thisType.GetMethod("Add", BindingFlags.Public ||| BindingFlags.Instance, null, keyValTypes, null)

      add.Invoke(parentInstance, [| keyInstance; valInstance |]) |> ignore
      struct (true, parentInstance)
    else
      struct (false, TypeHelper.getDefault thisType)
