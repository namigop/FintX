module Tefin.Core.Utils

open System
open System.Collections.Generic
open System.Diagnostics
open System.Globalization
open System.Reflection
open System.Runtime.InteropServices
open System.IO
open System.Text
open Newtonsoft.Json

let appName = "FintX"

let appVersion = Assembly.GetEntryAssembly().GetName().Version
let appVersionSimple = $"{appVersion.Major}.{appVersion.Minor}"

let some obj = Some obj
let none = None

let getCultures () =
    CultureInfo.GetCultures(CultureTypes.AllCultures)
    |> Seq.map (fun x -> x.LCID)
    |> Seq.distinct
    |> Seq.filter (fun x -> not (x = 4096))
    |> Seq.map (fun x -> CultureInfo.GetCultureInfo(x).Name)
    |> Seq.toArray

let getCompareOptions () =
    let names = Enum.GetNames(typeof<CompareOptions>)
    [| "" |] |> Array.append names

let getPages (pageSize: int) (total: int) =
    let pages = Dictionary<int, int * int>()

    if (pageSize > total) then
        pages[0] <- (0, total - 1)
    else
        let maxPageCount = (total / pageSize) + 1

        for i in 0 .. (maxPageCount - 1) do
            let pageStart = i * pageSize

            if (pageStart < total) then
                let pageEnd = pageStart + pageSize - 1
                pages[i] <- (pageStart, (if (pageEnd < total - 1) then pageEnd else total - 1))

    pages

let isWindows () =
    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)

let isMac () =
    RuntimeInformation.IsOSPlatform(OSPlatform.OSX)

let isLinux () =
    RuntimeInformation.IsOSPlatform(OSPlatform.Linux)

let getAppDataPath () =
    let get () =
        if isWindows () then
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
            |> fun p -> Path.Combine(p, appName)
        elif isMac () then
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            |> fun p -> Path.Combine(p, ".local", "share", appName)
        else
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            |> fun p -> Path.Combine(p, appName)

    get () |> (fun d -> Directory.CreateDirectory d) |> (fun d -> d.FullName)

let getTempPath () =
    Path.Combine(getAppDataPath (), "Temp")
    |> fun d -> Directory.CreateDirectory d
    |> fun d -> d.FullName

let bytesToBase64 (bytes: byte array) = Convert.ToBase64String(bytes)

let streamToBase64 (stream: Stream) =
    let _ = stream.Seek(0, SeekOrigin.Begin)
    use ms = new MemoryStream()
    stream.CopyTo(ms)
    let _ = ms.Seek(0, SeekOrigin.Begin)
    ms.ToArray() |> bytesToBase64

let fileToBase64 (file: string) =
    if (File.Exists file) then
        File.ReadAllBytes(file) |> bytesToBase64
    else
        ""

let printTimeSpan (elapsed: TimeSpan) =
    if (elapsed.TotalMinutes > 1) then
        let min = elapsed.TotalMinutes.ToString("0.#")
        $"{min} min"
    elif (elapsed.TotalSeconds > 1) then
        let sec = elapsed.TotalSeconds.ToString("0.#")
        $"{sec} sec"
    elif (elapsed.TotalMilliseconds = 0) then
        $"---"
    else
        let ms = elapsed.TotalMilliseconds.ToString("0.#")
        $"{ms} msec"

let printFileSize (length: int64) =
    if (length < 1000L) then
        $"{length} B"
    else
        let kb = (Convert.ToDouble length) / 1024.0

        if (kb < 1000) then
            kb.ToString("0.#") + " kB"
        else
            let mb = (Convert.ToDouble length) / (1024.0 * 1024.0)
            mb.ToString("0.#") + " MB"

let makeValidFileName (name: string) =
    let builder = StringBuilder()
    let invalid = Path.GetInvalidFileNameChars()

    for c in name do
        match invalid |> Array.tryFind (fun i -> i = c) with
        | Some(d) -> builder.Append "_"
        | None -> builder.Append c
        |> ignore

    builder.ToString()

let cache fn =
    let dict = Dictionary<_, _>()

    fun c ->
        let exist, value = dict.TryGetValue c

        match exist with
        | true -> value
        | _ ->
            let value = fn c
            dict.Add(c, value)
            value

let openBrowser (url: string) =
    if isLinux () then
        Process.Start("xdg-open", url)
    else
        let psi = new ProcessStartInfo(FileName = url, UseShellExecute = true)
        Process.Start(psi)

let getFileName (path: string) (fileStart: string) (fileExt:string)=

    let existingFileNames =
        Directory.GetFiles(path, "*" + fileExt)
        |> Array.map (fun c -> Path.GetFileName c)

    
    let max = 1000000

    seq { for i in 0..max -> i }
    |> Seq.map (fun counter ->
        //Sample name : MethodName (1).frxq
        let targetName =
            if counter = 0 then
                $"{fileStart}{fileExt}"           
            else
                $"{fileStart}({counter}){fileExt}"
        targetName)
    |> Seq.filter (fun name ->
        let existingFile = existingFileNames |> Array.contains name
        not existingFile)
    |> Seq.head
