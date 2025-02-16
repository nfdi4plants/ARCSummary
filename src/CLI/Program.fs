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
            | CreateNewBranch branchArgs ->
                let personalAccessToken = branchArgs.GetResult BranchArgs.Token
                let pathOrId = branchArgs.GetResult BranchArgs.PathOrId
                let newBranchId = branchArgs.GetResult BranchArgs.NewBranch
                let main = branchArgs.GetResult BranchArgs.MainBranch
                let apiAddress = branchArgs.TryGetResult BranchArgs.APIAdress

                APIRequest.CreateNewBranch(personalAccessToken, pathOrId, newBranchId, main, ?apiAddress = apiAddress)  |> ignore
                printfn "New Branch has been created"
                0

            | CreateMR mRArgs ->
                let personalAccessToken= mRArgs.GetResult MRArgs.Token
                let pathOrId = mRArgs.GetResult MRArgs.PathOrId
                let sourceBranch = mRArgs.GetResult SourceBranch
                let main = mRArgs.GetResult MRArgs.MainBranch
                let title = mRArgs.GetResult MRArgs.CommitTitle
                let apiAddress = mRArgs.TryGetResult MRArgs.APIAdress
    
                APIRequest.CreateMR(personalAccessToken, pathOrId, sourceBranch, main, title, ?apiAddress = apiAddress) |> ignore
                printfn "Merge Request created successfully"
                0

            | CreateCommitWorkflow commitArgs ->
                let personalAccessToken = commitArgs.GetResult CommitArgs.Token
                let pathOrId = commitArgs.GetResult CommitArgs.PathOrId
                let main = commitArgs.GetResult CommitArgs.MainBranch                
                let newBranchId = commitArgs.GetResult CommitArgs.NewBranch
                let title = commitArgs.GetResult CommitArgs.CommitTitle
                let commitMesage = commitArgs.GetResult CommitMessage      
                let filePath = commitArgs.GetResult FilePath 
                let usrEmail = commitArgs.TryGetResult UserEmail
                let usrName = commitArgs.TryGetResult UserName     
                let force = commitArgs.TryGetResult Force
                let apiAddress = commitArgs.TryGetResult CommitArgs.APIAdress
                let content = 
                    WorkflowRequest.GetMarkdownContent(personalAccessToken, pathOrId, main,?apiAddress = apiAddress)       
                let updatedContent =
                    let arc = (ARC.load(filePath).ISA.Value)  
                    updateREADME  content arc
                WorkflowRequest.CreateCommit(pathOrId, main, newBranchId,commitMesage, filePath, updatedContent, ?userEmail = usrEmail, ?userName = usrName,?force = force, ?apiAddress = apiAddress) |> ignore
                printfn "Commit created successfully"
                0 |> ignore

                APIRequest.CreateMR(personalAccessToken, pathOrId, newBranchId, main, title, ?apiAddress = apiAddress) |> ignore
                printfn "Merge Request created  successfully"
                0


        with
        :? ArguParseException as e ->
            eprintfn "Error parsing arguments: %s" e.Message
            1 



