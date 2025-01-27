namespace Tefin.Core

open System.Threading.Tasks
open Tefin.Core.Infra.Actors
open Tefin.Core.Interop
open System.IO
open ClientStructure

module VarsStructure =
    
    let getVarPath (projectPath: string) = Path.Combine(projectPath, "vars")
    let getVarFiles (io:IOs) (projectPath: string) =
        let varPath = getVarPath projectPath
        let varFiles = io.Dir.GetFiles(varPath, "*" + Ext.envExt, SearchOption.TopDirectoryOnly)
        varFiles
    
    let getVars (io:IOs) (projectPath: string) =
        let files = getVarFiles io projectPath
        files
        |> Array.map (fun c -> io.File.ReadAllText c)
        |> Array.map (fun c ->  Instance.jsonDeserialize<EnvConfigData>(c))
         
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
        
     
    
