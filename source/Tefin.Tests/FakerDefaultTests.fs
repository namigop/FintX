module FakerDefaultTests

open System
open System.IO
open System.Text

open Tefin.Core
open Tefin.Core.Reflection

open Tefin.Tests.TestInputTypes

open Xunit

// An unpaired surrogate is not valid UTF-8. File.WriteAllText uses a strict encoder and throws
// EncoderFallbackException on one, which means a single bad generated value can take a whole
// request save down with it.
//
// The lone surrogate has to be built at runtime: a "\uD800" literal is written into the assembly's
// string heap as UTF-8, so the compiler replaces it with U+FFFD before the test ever runs.
let private loneSurrogate =
    let c = System.Convert.ToChar(0xD800)
    String([| c |])

let private loneSurrogateCount (s: string) =
    let mutable n = 0
    let mutable i = 0
    while i < s.Length do
        let c = s.[i]
        if Char.IsHighSurrogate(c) then
            if i + 1 >= s.Length || not (Char.IsLowSurrogate(s[i + 1])) then
                n <- n + 1
            else
                i <- i + 1
        elif Char.IsLowSurrogate(c) then
            n <- n + 1
        i <- i + 1
    n

//a single char is always alone - a valid surrogate pair needs two chars
let private isLoneSurrogateChar (c: char) = Char.IsSurrogate(c)

let private tryGet (fk: Faker.Fk) =
    try
        let v = fk.Get()
        if isNull v then None else Some v
    with _ ->
        None

// Bogus.Randomizer.String()/Utf16String() fill a buffer with random chars taken from the whole
// UTF-16 code unit range, surrogate block included, so they must never be reachable as a default
// value generator. A single sample is not enough - these generators only produce a lone surrogate
// some of the time - so every generator is sampled repeatedly.
[<Fact>]
let ``no faker generator emits an unpaired surrogate`` () =
    let offenders =
        Faker.fakerInfo.Values
        |> Seq.collect (fun fk ->
            seq {
                for _ in 1..16 do
                    match tryGet fk with
                    | Some (:? string as s) when loneSurrogateCount s > 0 -> yield fk.Name
                    | Some (:? char as c) when isLoneSurrogateChar c -> yield fk.Name
                    | _ -> ()
            })
        |> Seq.distinct
        |> Seq.toList

    Assert.True(offenders.IsEmpty, sprintf "these generators emit unpaired surrogates: %A" offenders)

[<Fact>]
let ``a string property named StringType gets a valid default`` () =
    for _ in 1..16 do
        let struct (ok, v) = Faker.getDefault "StringType" typeof<string> true None 0
        Assert.True(ok)
        Assert.NotNull(v)
        Assert.Equal(0, loneSurrogateCount (string v))

[<Fact>]
let ``a default message instance holds no unpaired surrogate`` () =
    for _ in 1..8 do
        let struct (ok, instance) = TypeBuilder.getDefault "" typeof<Test1> true None 0
        Assert.True(ok)
        Assert.NotNull(instance)

        for p in typeof<Test1>.GetProperties() do
            match p.GetValue(instance) with
            | :? string as s ->
                Assert.True(loneSurrogateCount s = 0, sprintf "%s holds an unpaired surrogate" p.Name)
            | :? char as c ->
                Assert.False(isLoneSurrogateChar c, sprintf "%s holds an unpaired surrogate" p.Name)
            | _ -> ()

// Even if a bad value reaches the writer, it must not take the save down with it.
[<Fact>]
let ``the BCL writer throws on a lone surrogate`` () =
    //documents why the lenient encoder is needed
    Assert.Equal(1, loneSurrogateCount loneSurrogate)

    let file = Path.Combine(Path.GetTempPath(), $"fintx-bcl-{Guid.NewGuid()}.txt")
    try
        Assert.Throws<EncoderFallbackException>(fun () ->
            System.IO.File.WriteAllText(file, "before" + loneSurrogate + "after"))
    finally
        File.Delete(file)

[<Fact>]
let ``writing text with an unpaired surrogate does not throw`` () =
    let io = Resolver.value
    let file = Path.Combine(Path.GetTempPath(), $"fintx-lenient-{Guid.NewGuid()}.txt")
    try
        io.File.WriteAllText file ("before" + loneSurrogate + "after")

        let written = File.ReadAllText(file)
        Assert.True(written.StartsWith "before")
        Assert.True(written.EndsWith "after")
    finally
        File.Delete(file)

[<Fact>]
let ``writing text asynchronously with an unpaired surrogate does not throw`` () =
    let io = Resolver.value
    let file = Path.Combine(Path.GetTempPath(), $"fintx-lenient-async-{Guid.NewGuid()}.txt")
    try
        io.File.WriteAllTextAsync file ("before" + loneSurrogate + "after")
        |> fun t -> t.GetAwaiter().GetResult()

        let written = File.ReadAllText(file)
        Assert.True(written.StartsWith "before")
        Assert.True(written.EndsWith "after")
    finally
        File.Delete(file)
