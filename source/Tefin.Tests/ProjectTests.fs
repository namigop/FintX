module ProjectTests

open System
open System.Text.RegularExpressions
open Microsoft.VisualBasic.FileIO
open Tefin.Core
open Tefin.Core.Interop
open Xunit
open System.IO;


let testProjectName = "myProject"
let testClientName = "myClient"
let testMethodName1 = "myMethod1"
let testMethodName2 = "myMethod1"
let testFile1Request = "file1.fxrq"
let testFile1Content = "{}"

let testProjSaveStateCotent = "{}"
let testClientConfigContent = "{}"

let testFile2Request = "file2.fxrq"
let testFile2Content = "{}"

type File =
  {
    Name:string
    Content :string
  }
  
type Folder =
  {
    Name : string
    Files : File array
    Folders : Folder array
  }
let projectFolder =
   (* The folder structure
    projects
       - {projectName}
         - projectState.json
         - {clientName}
            - config.json
            - code
            - methods
                - {methodName}
                   - _autoSave
                   - file1.fxrq
                   - file2.fxrq
    *)
    
  {
    Name = "projects"
    Files = Array.empty
    Folders = [|
      {
        Name = testProjectName
        Files = [| 
                    { Name = ProjectSaveState.FileName; Content =  testProjSaveStateCotent} |]
        Folders = [|
          {
            Name = testClientName
            Files = [|
              { Name =ClientGroup.ConfigFilename; Content = testClientConfigContent }
            |]
            Folders = [|
              {
                Name = "code"
                Files = [|
                  { Name = "Client.cs"; Content = "not used" }
                  { Name = "Client.g.cs"; Content = "not used" }
                |]
                Folders = Array.empty
              }
              {
                Name = "methods"
                Files = Array.empty
                Folders = [|
                  {
                    Name = testMethodName1
                    Files = [|
                      { Name = testFile1Request; Content =  testFile1Content}
                    |]
                    Folders = [|
                      {
                        Name= Project.AutoSaveFolderName
                        Files = Array.empty
                        Folders = Array.empty 
                      }
                    |]
                  }
                  {
                    Name = testMethodName2
                    Files = [|
                       { Name = testFile2Request; Content =  testFile2Content}
                    |]
                    Folders = [|
                      {
                        Name= Project.AutoSaveFolderName
                        Files = Array.empty
                        Folders = Array.empty 
                      }
                    |]
                  }
                |]
              }
              
            |]
          }
        |]
      }
    |]
  }
  
 
let mock =
  let matchFilePattern (file: File) (pattern:string) =
      if (pattern = "*.*") then
        true
      else
        let ext = Path.GetExtension file.Name
        file.Name.EndsWith(ext)
      
  let rec getDirRec (folder:Folder) (targetPath:string) (currentPath:string)  =
      if currentPath = targetPath then
        folder.Folders      
      else        
          folder.Folders
          |> Array.collect (fun f -> getDirRec f targetPath $"{currentPath}/{f.Name}")
       
  let rec getFilesRec (folder:Folder) pattern (option:SearchOption) (pathParts:string array) (p:string)  =
    if option = SearchOption.TopDirectoryOnly then
      if (pathParts[pathParts.Length-1] = folder.Name) then
        folder.Files
        |> Array.filter (fun f -> matchFilePattern f pattern)
        |> Array.map(fun f ->
             let p = String.Join("/", pathParts)
             let fullPath = $"{p}/{f.Name}"
             {f with Name = fullPath }
             )
      else        
          folder.Folders
          |> Array.collect (fun f -> getFilesRec f pattern option pathParts "")
          |> Array.filter (fun f -> matchFilePattern f pattern)
    else       
        folder.Files
        |> Array.filter (fun f -> matchFilePattern f pattern)
        |> Array.map(fun f ->
             let fullPath = $"{p}/{f.Name}"
             {f with Name = fullPath }
             )
        |> Array.append (
          folder.Folders
          |> Array.collect (fun f -> getFilesRec f pattern option pathParts ($"{p}/{f.Name}") ))         
           
  let getFiles (path:string, pattern:string, options:SearchOption) =
    let pathParts = path.Split("/")
    getFilesRec projectFolder pattern options pathParts projectFolder.Name
    
  let readAllText (file:string) =
    let dir = Path.GetDirectoryName file
    getFiles (dir, "*.*", SearchOption.AllDirectories)
    |> Array.tryFind(fun f -> f.Name = file)
    |> fun m ->
      match m with
      | Some(f) -> f.Content
      | None -> raise (FileNotFoundException("file not found", file))
      
  let createDirectory name =
    ()
  
  let getDirectories (targetPath:string) =
    getDirRec projectFolder targetPath ""
  
  getDirectories
                                                                                 
                                                                                    
      
      
    
    

  
(* (createDirectory: string -> unit)
    (getDirectories: string -> string array)*)
[<Fact>]
let ``Can build class instance`` () =
  // let file = $"projects/{testProjectName}/{testClientName}/methods/{testMethodName1}/{testFile1Request}"
  // let content = mock file
  //
   let dirs = mock ($"projects/{testProjectName}/{testClientName}/methods")
   ()
  // let duh =
  //   mock($"projects/{testProjectName}/{testClientName}", "*.*", SearchOption.SearchAllSubDirectories)
  // Assert.True(duh.Length > 1)




