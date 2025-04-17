namespace Tefin.Core

open System.Threading.Tasks
open Tefin.Core.Infra.Actors
open Tefin.Core.Infra.Actors.Logging
open Tefin.Core.Interop
open System.IO
open ClientStructure

module VarsStructure =
 
    
    let getVarPathForProject  (projectPath: string) =
        let dir = Path.Combine(projectPath, "vars")         
        dir
    let getVarFilesForProject (io:IOs) (projectPath: string) =
        let varPath = getVarPathForProject projectPath
        io.Dir.CreateDirectory varPath
        let varFiles = io.Dir.GetFiles(varPath, $"*{Ext.envExt}", SearchOption.TopDirectoryOnly)
        varFiles
    
    let getVarPathForClient (clientPath: string) =
        let dir = Path.Combine(clientPath, "vars")         
        dir
        
    let getVarFilesForClient (io:IOs) (clientPath: string) =
        let varPath = getVarPathForClient clientPath
        io.Dir.CreateDirectory varPath
        let varFiles = io.Dir.GetFiles(varPath, $"*{Ext.envExt}", SearchOption.TopDirectoryOnly)
        varFiles
    let getVarsFromFile (io:IOs) (file:string) =
        let c = io.File.ReadAllText file
        let data = Instance.jsonDeserialize<EnvConfigData>(c)
        data
    let getVarsForProject (io:IOs) (projectPath: string) =
        let files = getVarFilesForProject io projectPath         
        files        
        |> Array.map (fun file -> file, getVarsFromFile io file)
        |> Array.filter (fun (file, env) -> not ((box env) = null))
        |> fun v ->
            let allEnvs = { Path = projectPath; Variables = ResizeArray<string * EnvConfigData> () }
            for (file, c) in v do
               let e = file, c
               allEnvs.Variables.Add(e)            
            allEnvs
    
    let getVarsForProjectEnv (io:IOs) (projectPath: string) (envName:string) =
        let all = getVarsForProject io projectPath
        all.Variables
        |> Seq.tryFind (fun (_, data) -> data.Name = envName)
        |> function
           | Some s -> s
           | None -> "", EnvConfig.createConfig envName "no variables defined"
    
    let getVarsForClient (io:IOs) (clientPath: string) =
        let files = getVarFilesForClient io clientPath         
        files        
        |> Array.map (fun file -> file, getVarsFromFile io file)
        |> fun v ->
            let allEnvs = {Path = clientPath; Variables = ResizeArray<string * EnvConfigData> () }
            for (file, c) in v do
               let e = file, c
               allEnvs.Variables.Add(e)            
            allEnvs
    let getVarsForClientEnv (io:IOs) (clientPath: string) (envName:string) =
        let all = getVarsForClient io clientPath
        all.Variables
        |> Seq.tryFind (fun (_, data) -> data.Name = envName)
        |> function
           | Some s -> s
           | None ->
               let file = Path.Combine ( (getVarPathForClient clientPath), envName + Ext.envExt )
               let data = EnvConfig.createConfig envName "no variables defined"
               file, data
    let saveEnvForClient (io:IOs) (cfg:EnvConfigData) (clientPath:string) =
        let path = getVarPathForClient clientPath
        let envFile = Path.Combine(path, cfg.Name)                    
        io.File.WriteAllText envFile (Instance.jsonSerialize cfg)
        
    let updateEnvForClient (io:IOs) (envName:string) (envVar:EnvVar) (clientPath:string) =
        let all = getVarsForClient io clientPath
        let envConfig = all.Variables |> Seq.tryFind  (fun (_, data) -> data.Name = envName)
        match envConfig with
        | None -> ()
        | Some (envFile, cfg) ->
            let curVar = cfg.Variables |> Seq.tryFind (fun v -> v.Name = envVar.Name)
            if curVar.IsSome then
                ignore <| cfg.Variables.Remove(curVar.Value)
                cfg.Variables.Add envVar
                io.File.WriteAllText envFile (Instance.jsonSerialize cfg)
        
    let saveEnvForProject (io:IOs) (cfg:EnvConfigData) projectPath =
        let path = getVarPathForProject projectPath 
        let file = Utils.makeValidFileName(cfg.Name + Ext.envExt)
        let envFile = Path.Combine(path, file)
        io.File.WriteAllText envFile (Instance.jsonSerialize cfg)
    
    let demo() =
        let vars = EnvConfig.createConfig "UAT" "UAT env variables"
        vars.Variables.Add( {Name = "{{FooVar}}"
                             CurrentValue = ""
                             DefaultValue = "defaultvalue"
                             Description = "test desc"
                             Type = "System.String"} )
        vars.Variables.Add( {Name = "{{BarVar}}"
                             CurrentValue = "Bar"
                             DefaultValue = "defaultvalue"
                             Description = "test desc"
                             Type = "System.Int32"} )
        vars.Variables.Add( {Name = "{{BazVar}}"
                             CurrentValue = "True"
                             DefaultValue = "defaultvalue"
                             Description = "test desc"
                             Type = "System.Boolean"} )
        
        let json = Instance.jsonSerialize vars
        json
        //Instance.jsonDeserialize
        
     
    
