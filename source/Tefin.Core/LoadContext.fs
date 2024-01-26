namespace Tefin.Core

open System.Runtime.Loader
open System.Reflection

type LoadContext() =
  inherit AssemblyLoadContext()

  override x.Load(assemblyName: AssemblyName) : Assembly = Unchecked.defaultof<Assembly>
