module Tefin.Core.Utils

open System
open System.Collections.Generic
open System.Diagnostics
open System.Globalization
open System.Reflection
open System.Runtime.InteropServices
open System.IO
open System.Text
open System.Security.Cryptography
open Newtonsoft.Json.Linq

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

let private deriveKeyAndIV (password: string) (salt: byte[]) =
    use deriveBytes = new Rfc2898DeriveBytes(password, salt, 1000, HashAlgorithmName.SHA256)
    (deriveBytes.GetBytes(32), deriveBytes.GetBytes(16))

let getMD5Hash (file:string) =
    use md5 = MD5.Create()
    use stream = File.OpenRead(file)
    let hash = md5.ComputeHash(stream)
    BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()
  
let encrypt (text: string) (password: string) =
    let bytes = Encoding.UTF8.GetBytes(text)
    let salt = Array.zeroCreate<byte> 16
    use rng = RandomNumberGenerator.Create()
    rng.GetBytes(salt)
    
    let (key, iv) = deriveKeyAndIV password salt
    use aes = Aes.Create()
    use encryptor = aes.CreateEncryptor(key, iv)
    use msEncrypt = new MemoryStream()
    use csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)
    csEncrypt.Write(bytes, 0, bytes.Length)
    csEncrypt.FlushFinalBlock()
    
    // Prepend salt to encrypted data
    let encryptedBytes = msEncrypt.ToArray()
    let result = Array.concat [salt; encryptedBytes]
    Convert.ToBase64String(result)

let decrypt (encryptedText: string) (password: string) =
    let allBytes = Convert.FromBase64String(encryptedText)
    
    // Extract salt (first 16 bytes) and encrypted data
    let salt = allBytes.[0..15]
    let encryptedBytes = allBytes.[16..]
    
    let (key, iv) = deriveKeyAndIV password salt
    use aes = Aes.Create()
    use decryptor = aes.CreateDecryptor(key, iv)
    use msDecrypt = new MemoryStream(encryptedBytes)
    use csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)
    use msResult = new MemoryStream()
    csDecrypt.CopyTo(msResult)
    Encoding.UTF8.GetString(msResult.ToArray())
  
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

let openPath (path: string) =
  if isWindows() then
    Process.Start("explorer.exe", path)
  elif isMac() then
    Process.Start("open", path)
  else
    Process.Start("xdg-open", path)
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

let getAvailableFileName (path: string) (fileStart: string) (fileExt: string) =
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

let jSelectToken (json: string) (jPath: string) =
  let jObj = JObject.Parse json
  (jObj.SelectToken jPath)

let jSelectTokens (json: string) (jPath: string) =
  let jObj = JObject.Parse json
  (jObj.SelectTokens jPath)
  
let calculateSimilarity (text1:string) (text2:string) : float=
  // Calculate the similarity score using Jaro-Winkler algorithm
  if String.IsNullOrEmpty(text1) && String.IsNullOrEmpty(text2) then 1.0
  elif text1 = text2 then 1.0
  elif String.IsNullOrEmpty(text1) || String.IsNullOrEmpty(text2) then 0.0  
  else
    let s1 = text1.ToLowerInvariant()
    let s2 = text2.ToLowerInvariant()
    let len1 = s1.Length
    let len2 = s2.Length
          
    // Calculate match window
    let matchWindow = (max len1 len2) / 2 - 1
    let matchWindow = if matchWindow < 0 then 0 else matchWindow
    
    let s1Matches = Array.create len1 false
    let s2Matches = Array.create len2 false
    
    let mutable matches = 0
    let mutable transpositions = 0
    
    // Identify matches
    for i in 0 .. len1 - 1 do
      let start = max 0 (i - matchWindow)
      let endIdx = min (i + matchWindow + 1) len2
      
      for j in start .. endIdx - 1 do
        if not s2Matches.[j] && s1.[i] = s2.[j] then
          s1Matches.[i] <- true
          s2Matches.[j] <- true
          matches <- matches + 1
    
    if matches = 0 then 0
    else
      // Count transpositions
      let mutable k = 0
      for i in 0 .. len1 - 1 do
        if s1Matches.[i] then
          while not s2Matches.[k] do
            k <- k + 1
          if s1.[i] <> s2.[k] then
            transpositions <- transpositions + 1
          k <- k + 1
      
      let m = float matches
      let t = (float transpositions) / 2.0
      
      // Calculate Jaro similarity
      let jaro = ((m / float len1) + (m / float len2) + ((m - t) / m)) / 3.0
      
      // Calculate common prefix length (up to 4 characters)
      let mutable prefixLen = 0
      let maxPrefix = min 4 (min len1 len2)
      while prefixLen < maxPrefix && s1.[prefixLen] = s2.[prefixLen] do
        prefixLen <- prefixLen + 1
      
      // Calculate Jaro-Winkler similarity
      let p = 0.1 // standard scaling factor
      jaro + (float prefixLen * p * (1.0 - jaro))
  
let splitWord (text:string) =
    //split a camel-cased or Pascal-cased string
    // "StreetAddress" -> "Street" and "Address"
    //  "addressPostalCode" -> "address", "Postal", "Code"
    if System.String.IsNullOrWhiteSpace(text) then
      [||]
    else
      let mutable result = []
      let mutable currentWord = StringBuilder()
    
      for i = 0 to text.Length - 1 do
        let c = text.[i]
      
        if i = 0 then
          currentWord.Append(c) |> ignore
        elif Char.IsUpper(c) || c = '_' then
          // Start a new word when we encounter an uppercase letter
          if currentWord.Length > 0 then
            result <- currentWord.ToString() :: result
            currentWord.Clear() |> ignore
          currentWord.Append(c) |> ignore
        else
          currentWord.Append(c) |> ignore
    
      // Add the last word
      if currentWord.Length > 0 then
        result <- currentWord.ToString() :: result
    
      result |> List.rev |> List.toArray