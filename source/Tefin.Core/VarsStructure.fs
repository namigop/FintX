namespace Tefin.Core

open System.Threading.Tasks
open Tefin.Core.Infra.Actors
open Tefin.Core.Infra.Actors.Logging
open Tefin.Core.Interop
open System.IO
open ClientStructure

module VarsStructure =
    
    let getVarPath  (projectPath: string) =
        let dir = Path.Combine(projectPath, "vars")         
        dir
    let getVarFiles (io:IOs) (projectPath: string) =
        let varPath = getVarPath projectPath
        io.Dir.CreateDirectory varPath
        let varFiles = io.Dir.GetFiles(varPath, "*" + Ext.envExt, SearchOption.TopDirectoryOnly)
        varFiles
    
    let getVarsFromFile (io:IOs) (file:string) =
        let c = io.File.ReadAllText file
        let data = Instance.jsonDeserialize<EnvConfigData>(c)
        data
    let getVars (io:IOs) (projectPath: string) =
        let files = getVarFiles io projectPath         
        files        
        |> Array.map (fun file -> file, getVarsFromFile io file)
        |> fun v ->
            let projEnvs = { Variables = ResizeArray<string * EnvConfigData> () }
            for (file, c) in v do
               let e = file, c
               projEnvs.Variables.Add(e)            
            projEnvs
         
    let saveEnv (io:IOs) (cfg:EnvConfigData) projectPath =
        let path = getVarPath projectPath 
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
        
     
    
