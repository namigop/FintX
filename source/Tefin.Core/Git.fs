namespace Tefin.Core.Git

open LibGit2Sharp
open System
open System.IO

type FileGitStatus =
    | Untracked
    | Unmodified
    | Modified
    | Added
    | Deleted
    | Renamed    
    | Ignored
    | NoRepository

module Git =
    let private getRepositoryPath (filePath: string) =
        try
            Repository.Discover(Path.GetDirectoryName(filePath))
        with
        | _ -> null

    let getFileStatus (filePath: string) =
        let repoPath = getRepositoryPath filePath
        
        if String.IsNullOrEmpty(repoPath) then
            NoRepository
        else
            use repo = new Repository(repoPath)
            let repoRelativePath = Path.GetRelativePath(repo.Info.WorkingDirectory, filePath)          
            let status = repo.RetrieveStatus(repoRelativePath)
            
            match status with
            //| FileStatus .Untracked -> Untracked
            | FileStatus.Ignored -> Ignored
            | FileStatus.Unaltered -> Unmodified
            | FileStatus.NewInIndex -> Added
            | FileStatus.NewInWorkdir -> Added
            | FileStatus.DeletedFromIndex -> Deleted
            | FileStatus.DeletedFromWorkdir -> Deleted
            | FileStatus.RenamedInIndex -> Modified
            | FileStatus.ModifiedInIndex -> Modified
            | FileStatus.ModifiedInWorkdir -> Modified
            | FileStatus.TypeChangeInIndex -> Modified
            | FileStatus.TypeChangeInWorkdir -> Modified
            | _ -> Untracked

     
//
// // Example usage module
// module Program =
//     [<EntryPoint>]
//     let main argv =
//         let filePath = @"C:\YourProject\SomeFile.txt"
//
//         // Check basic status
//         let status = GitStatusChecker.getFileStatus filePath
//         printfn "File Status: %A" status
//
//         // Get detailed status
//         GitStatusChecker.getDetailedFileStatus filePath
//
//         // Check for uncommitted changes
//         let hasUncommittedChanges = 
//             GitStatusChecker.hasUncommittedChanges filePath
//         
//         printfn "Has Uncommitted Changes: %b" hasUncommittedChanges
//
//         0 // Return exit code