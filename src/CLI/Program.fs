namespace ARCSummary


open Argu


module prototypeCLI =

    type CLIArguments =
        // could be mandatory
        | ARC_Directory of arcPath : string 
        // add some more arguments such as specified help or selected content like only assay or study overview

        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | ARC_Directory  _ -> "specify your ARC directory" 



// https://fsprojects.github.io/Argu/tutorial.html
// https://www.nuget.org/packages/Argu
// For more information see https://aka.ms/fsharp-console-apps
// printfn "Hello from F#"