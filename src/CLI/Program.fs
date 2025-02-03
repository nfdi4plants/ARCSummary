namespace ARCSummary


open Argu


module prototypeCLI =

    type CLIArguments =
        // could be mandatory
        | [<Mandatory>] ARC_Directory of arcPath : string 
        // add some more arguments such as specified help or selected content like only assay or study overview
        | [<Mandatory>] CreateMR of pathOrId: string * newBranch: string * main: string * commitMessage: string
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | ARC_Directory  _ -> "specify your ARC directory" 
                | CreateMR _ -> "creates a new merge request. requires: <projectPathOrId> <newBranch> <mainBranch> <commitMessage>"


// https://fsprojects.github.io/Argu/tutorial.html
// https://www.nuget.org/packages/Argu
// For more information see https://aka.ms/fsharp-console-apps
// printfn "Hello from F#"