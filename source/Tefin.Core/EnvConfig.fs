namespace Tefin.Core

open System.Collections.Generic

type EnvVar = {
    Name: string
    CurrentValue: string
    DefaultValue: string
    Description: string
    Type:string
}

type EnvConfigData = {   
    Name: string    
    Description: string    
    Variables: ResizeArray<EnvVar>
}   

type ProjectEnvConfigData = {      
    Variables: ResizeArray<string * EnvConfigData>
}

module EnvConfig =
    let createVar name  defaultValue curValue desc dataType =
        let envVar = { Name = name
                       CurrentValue = curValue
                       DefaultValue = defaultValue
                       Description = desc
                       Type = dataType }        
        envVar
       
    let createConfig name desc =
        { Name = name; Description = desc; Variables = ResizeArray<EnvVar>() }