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
        let varFiles = io.Dir.GetFiles(varPath, "*.fxv" + Ext.envExt, SearchOption.TopDirectoryOnly)
        varFiles
    
    let getVarPathForClient (clientPath: string) =
        let dir = Path.Combine(clientPath, "vars")         
        dir
        
    let getVarFilesForClient (io:IOs) (clientPath: string) =
        let varPath = getVarPathForClient clientPath
        io.Dir.CreateDirectory varPath
        let varFiles = io.Dir.GetFiles(varPath, "*.fxv" + Ext.envExt, SearchOption.TopDirectoryOnly)
        varFiles
    let getVarsFromFile (io:IOs) (file:string) =
        let c = io.File.ReadAllText file
        let data = Instance.jsonDeserialize<EnvConfigData>(c)
        data
    let getVarsForProject (io:IOs) (projectPath: string) =
        let files = getVarFilesForProject io projectPath         
        files        
        |> Array.map (fun file -> file, getVarsFromFile io file)
        |> fun v ->
            let allEnvs = { Path = projectPath; Variables = ResizeArray<string * EnvConfigData> () }
            for (file, c) in v do
               let e = file, c
               allEnvs.Variables.Add(e)            
            allEnvs
    
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
    let saveEnvForClient (io:IOs) (cfg:EnvConfigData) (clientPath:string) =
        let path = getVarPathForClient clientPath
        let envFile = Path.Combine(path, cfg.Name)                    
        io.File.WriteAllText envFile (Instance.jsonSerialize cfg)
        
    let updateEnvForClient (io:IOs) (envName:string) (envVar:EnvVar) (clientPath:string) =
        let all = getVarsForClient io clientPath
        let envConfig = all.Variables |> Seq.tryFind  (fun (_, data) -> data.Name = envName)
        match envConfig with
        | None -> ()
        | Some (_, cfg) ->
            let curVar = cfg.Variables |> Seq.tryFind (fun v -> v.Name = envVar.Name)
            if curVar.IsSome then
                ignore <| cfg.Variables.Remove(curVar.Value)
                cfg.Variables.Add envVar
        updateonokclick
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
        
     
    
