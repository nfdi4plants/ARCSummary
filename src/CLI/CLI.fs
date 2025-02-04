namespace ARCSummary

open ARCtrl
open Argu
open mainCLI
open READMEAutomation
open MergeRequest

module CLI =

    [<EntryPoint>]
    let main args =
        let parser = ArgumentParser.Create<CLIArgs>(programName = "ARCSummary")
        try
            let res = parser.ParseCommandLine(args, raiseOnUsage = true)

            match res.GetSubCommand() with
            | Summary summaryArgs -> 
                match summaryArgs.TryGetResult ARC_Directory with
                | Some arcPath  ->
                    match ARC.load(arcPath).ISA with
                    | Some investigation ->
                        updateREADME arcPath investigation |> ignore
                        printfn "README.md updated successfully at %s" arcPath
                        0 
                    | None ->
                        printfn "Failed to load investigation from ARC at %s" arcPath
                        1 
                | None -> 
                    printfn "Invalid arguments for summary.\n\n%s" (parser.PrintUsage())
                    1
            | CreateMR mRArgs ->
                let pathOrId = mRArgs.GetResult PathOrID
                let sourceBranch = mRArgs.GetResult SourceBranch
                let main = mRArgs.GetResult MainBranch
                let title = mRArgs.GetResult CommitTitle

                createMR pathOrId sourceBranch main title |> ignore
                printfn "Merge Request created successfully"
                0
            | CreateNewBranch branchArgs ->
                let pathOrId = branchArgs.GetResult PathOrId
                let newBranchId = branchArgs.GetResult NewBranch
                let main = branchArgs.GetResult RefBranch

                createNewBranch pathOrId newBranchId main |> ignore
                printfn "New Branch has been created"
                0
        with
        :? ArguParseException as e ->
            eprintfn "Error parsing arguments: %s" e.Message
            1 



