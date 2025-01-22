namespace ARCSummary

open ARCtrl
open Argu
open prototypeCLI
open READMEAutomation

module CLI =

    [<EntryPoint>]
    let main args =
        let parser = ArgumentParser.Create<CLIArguments>(programName = "ARCSummary")
        try
            let res = parser.ParseCommandLine(args)
            
            match res.TryGetResult ARC_Directory with
            // check if a valid directory has been found
            | Some arcPath ->
                match ARC.load(arcPath).ISA with
                //further check if investigation can be accessed and used to append the markdown
                | Some investigation ->
                    updateREADME arcPath investigation |> ignore
                    printfn "README.md updated successfully at %s" arcPath
                    0 
                | None ->
                    printfn "Failed to load investigation from ARC at %s" arcPath
                    1 
            | None ->
                printfn "Invalid arguments.\n\n%s" (parser.PrintUsage())
                1 
        with
        | :? ArguParseException as errorMessage ->
            eprintfn "Error parsing arguments: %s" errorMessage.Message
            1 
        | ex ->
            eprintfn "Unexpected error: %s" ex.Message
            1 



