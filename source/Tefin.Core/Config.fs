module Tefin.Core.Config

open System.IO

let appName = "Fintx"

let projectsPath = Utils.getAppDataPath () |> fun p -> Path.Combine(p, "projects")
let defaultProjectPath = Path.Combine(projectsPath, "default")
let configFile = Path.Combine(Utils.getAppDataPath (), "config", $"app.config")
