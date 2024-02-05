module ProjectTests

open System
open Microsoft.VisualBasic.FileIO
open Tefin.Core
open Tefin.Core.Interop
open Xunit


let testProjectName = "myProject"
let testClientName = "myClient"
let testMethodName1 = "myMethod1"
let testMethodName2 = "myMethod1"
let testFile1Request = "file1.fxrq"
let testFile1Content = "{}"

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
    Files : string array
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
        Files = [| ProjectSaveState.FileName |]
        Folders = [|
          {
            Name = testClientName
            Files = [| ClientGroup.ConfigFilename |]
            Folders = [|
              {
                Name = "code"
                Files = [| "Client.cs"; "Client.g.cs" |]
                Folders = Array.empty
              }
              {
                Name = "methods"
                Files = Array.empty
                Folders = [|
                  {
                    Name = testMethodName1
                    Files = [|testFile1Request|]
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
                    Files = [|testFile2Request|]
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
  let rec getFilesRec (folder:Folder) (option:SearchOption) (pathParts:string array) (p:string)  =
    if option = SearchOption.SearchTopLevelOnly then
      if (pathParts[pathParts.Length-1] = folder.Name) then
        folder.Files
      else        
          folder.Folders
          |> Array.collect (fun f -> getFilesRec f option pathParts "")
    else       
        folder.Files
        |> Array.map(fun f -> $"{p}/{f}")
        |> Array.append (
          folder.Folders
          |> Array.collect (fun f -> getFilesRec f option pathParts ($"{p}/{f.Name}") ))         
           
  let getFiles (path:string, pattern:string, options:SearchOption) =
    let pathParts = path.Split("/")
    getFilesRec projectFolder options pathParts projectFolder.Name
    
  let readAllText (file:string) =
    

  

[<Fact>]
let ``Can build class instance`` () =
  let duh =
    mock($"projects/{testProjectName}/{testClientName}", "*.*", SearchOption.SearchAllSubDirectories)
  Assert.True(duh.Length > 1)




