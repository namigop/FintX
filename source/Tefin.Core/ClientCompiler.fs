namespace Tefin.Core.Build

open System
open System.IO
open System.Runtime.InteropServices.JavaScript
open Microsoft.CodeAnalysis
open System.Collections.Generic
open Tefin.Core
open Tefin.Core.Res
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.Text

type CompileInput = {
        ModuleFile :string
        SourceFiles : string array
        ReferencedFiles : string array
        TargetOutput : OutputKind
        AdditionalReferences : MetadataReference array
    }
    type CompileOutput = {
        CompilationErrors : string array
        CompiledBytes : byte array
        Success : bool
        TargetOutput : OutputKind
        Input : CompileInput option
    }

module ClientCompiler =

    let private getInternalReferencedFiles() =
        let referencedFiles =  List<string>()
            // referencedFiles.Add(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location);

        let fullName = typeof<Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo>.Assembly.FullName;
        let dll = (fullName.Split(",") |> Array.head ) + ".dll"
        referencedFiles.Add(Path.Combine(AppContext.BaseDirectory, dll))
        let platformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES").ToString().Split(Path.PathSeparator)
        for fullPath in platformAssemblies do
            let fileName = Path.GetFileName(fullPath)
            if (fileName.StartsWith("System") ||
                fileName.StartsWith("mscorlib") ||
                fileName.StartsWith("netstandard") ||
                fileName.StartsWith("Microsoft.")) then
                referencedFiles.Add(fullPath);
        referencedFiles

    let private createCompilation (io:IOResolver) (sourceFiles: string array) (assemblyOrModuleName:string) (outputKind:OutputKind)   (additionalReferences: MetadataReference array) =
       let syntaxTrees = sourceFiles |> Array.map (fun cs ->
            let codeString = SourceText.From(io.File.ReadAllText cs)
            let options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest)
            let parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options)
            parsedSyntaxTree
            )
       let referencedFiles = getInternalReferencedFiles()
       referencedFiles.AddRange(referencedFiles)
       let references =
         referencedFiles
         |> Seq.filter (fun f -> io.File.Exists f)
         |> Seq.map (fun refFile -> MetadataReference.CreateFromFile refFile)
         |> Seq.map (fun p -> p :> MetadataReference)
         |> Seq.append additionalReferences
         |> Seq.toArray
       CSharpCompilation.Create(assemblyOrModuleName,
                syntaxTrees,
                references,
                CSharpCompilationOptions(outputKind,
                     optimizationLevel = OptimizationLevel.Release,
                     assemblyIdentityComparer = DesktopAssemblyIdentityComparer.Default))
    let emptyOutput =
        {
        CompilationErrors = Array.empty
        CompiledBytes = Array.empty
        Success =false
        TargetOutput = OutputKind.DynamicallyLinkedLibrary
        Input = None }

    let compile (io:IOResolver) (input : CompileInput) =
        use ms = new MemoryStream()
        let moduleName = Path.GetFileName input.ModuleFile
        let compilation = createCompilation io input.SourceFiles moduleName input.TargetOutput input.AdditionalReferences
        let result = compilation.Emit ms

        if (result.Success) then
            ms.Seek(0, SeekOrigin.Begin) |> ignore
            let compiledBytes = ms.ToArray()
            let res : CompileOutput = {
                 CompilationErrors = Array.empty
                 CompiledBytes = compiledBytes
                 Success = true
                 TargetOutput = input.TargetOutput
                 Input = Some input
             }
            res
        else
             let failures = result.Diagnostics
                            |> Seq.filter (fun diagnostic ->
                                diagnostic.IsWarningAsError || diagnostic.Severity = DiagnosticSeverity.Error)
             let errors = failures
                          |> Seq.map (fun diagnostic -> $"{diagnostic.Id}, {diagnostic.GetMessage()}")

             let res : CompileOutput = {
                 CompilationErrors = errors |> Seq.toArray
                 CompiledBytes = Array.empty
                 Success = false
                 TargetOutput = input.TargetOutput
                 Input = Some input}
             res

    let getTypes (bytes: byte array) =
        use ms = new MemoryStream(bytes)
        ms.Seek(0, SeekOrigin.Begin) |>ignore
        let context = LoadContext()
        let assembly = context.LoadFromStream ms
        assembly.GetTypes()