namespace ARCSummary

open ARCtrl
open Argu
open mainCLI
open READMEAutomation
open ARCSummary.GitLabAPI
open SummaryStyles
open System.IO
open ConfigFileTypes
open ConfigFileDecode
open ARCtrl.Yaml
open YAMLicious
open Prompt

module CLI =

    [<EntryPoint>]
    let main args =
        let parser = ArgumentParser.Create<CLIArgs>(programName = "ARCSummary")
        try
            let res = parser.ParseCommandLine(args, raiseOnUsage = true)
            match res.GetSubCommand() with
            | Summary summaryArgs -> 
                let arcPath = summaryArgs.GetResult SummaryArgs.ARC_Directory
                let sectionOrder = loadSectionOrder arcPath
                match ARC.tryLoadAsync(arcPath) |> Async.RunSynchronously  with
                | Ok arc ->            
                    updateREADME sectionOrder arcPath arc |> ignore
                    printfn "README.md updated successfully at %s" arcPath
                    0 
                | Error msgs ->
                    printfn "Could not load ARC from %s:" arcPath
                    msgs
                    |> Array.iter (printfn "%s")
                    1
            | SummaryMR summaryMRArgs ->
                let arcPath = summaryMRArgs.GetResult SummaryMRArgs.ARC_Directory
                let personalAccessToken = summaryMRArgs.GetResult SummaryMRArgs.Token
                let pathOrId = summaryMRArgs.GetResult SummaryMRArgs.PathOrId
                let commitMessage = 
                    summaryMRArgs.TryGetResult SummaryMRArgs.CommitMessage
                    |> Option.defaultValue "Updated Readme with automatic summary."
                let title = 
                    summaryMRArgs.TryGetResult SummaryMRArgs.MRTitle
                    |> Option.defaultValue "Automatic README Summary Update"
                let sourceBranch = 
                    summaryMRArgs.TryGetResult SummaryMRArgs.SourceBranch
                    |> Option.defaultValue "arc-summary"
                let targetBranch = 
                    summaryMRArgs.TryGetResult SummaryMRArgs.TargetBranch
                    |> Option.defaultValue "main"
                let userName = 
                    summaryMRArgs.TryGetResult SummaryMRArgs.UserName
                    |> Option.defaultValue "ARC-Summary Bot"
                let userEmail =
                    summaryMRArgs.TryGetResult SummaryMRArgs.UserEmail
                let apiAddress = summaryMRArgs.TryGetResult SummaryMRArgs.APIAdress

                let readmePath = System.IO.Path.Combine(arcPath, "README.md")
                let readmeContent = System.IO.File.ReadAllText(readmePath)
                let action_Create = CommitAction.create ActionType.Create "README.md" readmeContent
                let action_Update = CommitAction.create ActionType.Update "README.md" readmeContent

                printfn "Create new Branch"
                                    
                let branchResponse = 
                    Branch.CreateNewBranch(personalAccessToken, pathOrId, sourceBranch, targetBranch, ?apiAddress = apiAddress)
                if branchResponse.StatusCode <> 201 then
                    printfn "Branch creation failed with: \n%s" (branchResponse.Body.ToString())

                printfn "Create Commit"
                let commitResponse = 
                    Commit.CreateCommit(personalAccessToken, pathOrId, sourceBranch, commitMessage, [action_Create], ?userEmail = userEmail, userName = userName, ?apiAddress = apiAddress)
                if commitResponse.StatusCode <> 201 && commitResponse.Body.ToString().Contains("A file with this name already exists") then
                    printfn "File already exists, updating instead"
                    let commitResponse = Commit.CreateCommit(personalAccessToken, pathOrId, sourceBranch, commitMessage, [action_Update], ?userEmail = userEmail, userName = userName, ?apiAddress = apiAddress)
                    if commitResponse.StatusCode <> 201 then
                        printfn "Commit creation failed with: \n%s" (commitResponse.Body.ToString())

                printfn "Create MR"
                let mergeResponse = MergeRequest.CreateMR(personalAccessToken, pathOrId, sourceBranch, targetBranch, title, ?apiAddress = apiAddress)
                if mergeResponse.StatusCode <> 201 then
                    printfn "Merge Request creation failed with: \n%s" (mergeResponse.Body.ToString())
                    1
                else 
                    0
            | BasicPrompt summaryArgs -> 
                let arcPath = summaryArgs.GetResult SummaryArgs.ARC_Directory
                match ARC.tryLoadAsync(arcPath) |> Async.RunSynchronously  with
                | Ok arc ->      
                    let prompt =       
                        basicPrompt arc 
                    printfn "%s" (prompt.ToString())
                    0 
                | Error msgs ->
                    printfn "Could not load ARC from %s:" arcPath
                    msgs
                    |> Array.iter (printfn "%s")
                    1
            | EnhancedPrompt summaryArgs -> 
                let arcPath = summaryArgs.GetResult SummaryArgs.ARC_Directory
                match ARC.tryLoadAsync(arcPath) |> Async.RunSynchronously  with
                | Ok arc ->      
                    let prompt =       
                        enhancedPrompt arc 
                    printfn "%s" (prompt.ToString())
                    0 
                | Error msgs ->
                    printfn "Could not load ARC from %s:" arcPath
                    msgs
                    |> Array.iter (printfn "%s")
                    1
            | HybridPrompt summaryArgs -> 
                let arcPath = summaryArgs.GetResult SummaryArgs.ARC_Directory
                match ARC.tryLoadAsync(arcPath) |> Async.RunSynchronously  with
                | Ok arc ->      
                    let prompt =       
                        hybridPrompt arc 
                    printfn "%s" (prompt.ToString())
                    0 
                | Error msgs ->
                    printfn "Could not load ARC from %s:" arcPath
                    msgs
                    |> Array.iter (printfn "%s")
                    1

        with
        :? ArguParseException as e ->
            eprintfn "Error parsing arguments: %s" e.Message
            1 



