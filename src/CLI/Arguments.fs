namespace ARCSummary


open Argu


module mainCLI =
    
    type SummaryArgs =
        | [<Mandatory>] [<AltCommandLine("-ap")>] ARC_Directory of arcPath : string 
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | ARC_Directory  _ -> "Specify your ARC directory" 

    and MRArgs = 
        | [<Mandatory>] [<AltCommandLine("-t")>] Token of string
        | [<Mandatory>] [<AltCommandLine("-pi")>] PathOrId of string 
        | [<Mandatory>] [<AltCommandLine("-sb")>] SourceBranch of string 
        | [<Mandatory>] [<AltCommandLine("-mb")>] MainBranch of string
        | [<Mandatory>] [<AltCommandLine("-ct")>] CommitTitle of string
        | [<EqualsAssignment>] [<AltCommandLine("-aa")>] APIAdress of string
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Token _ -> "Personal access token for gitlab"
                | PathOrId _ -> "ID or URL-encdoded path of the project after .org/"
                | SourceBranch _ -> "Name of the source branch"
                | MainBranch _ -> "Name of the target branch"
                | CommitTitle _ -> "Title of the MR"
                | APIAdress _ -> "Address of API, default is "
                // consider adding description as optional parameter
    
    and BranchArgs =
        | [<Mandatory>] [<AltCommandLine("-t")>] Token of string
        | [<Mandatory>] [<AltCommandLine("-pi")>] PathOrId of string 
        | [<Mandatory>] [<AltCommandLine("-nb")>] NewBranch of string 
        | [<Mandatory>] [<AltCommandLine("-mb")>] MainBranch of string 
        | [<EqualsAssignment>] [<AltCommandLine("-aa")>] APIAdress of string
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Token _ -> "Personal access token for gitlab"
                | PathOrId _ -> "ID or URL-encdoded path of the project after .org/"
                | NewBranch _ -> "Name of the new branch"
                | MainBranch _ -> "Name of the target branch usally main"
                | APIAdress _ -> "Testing Server URL"
                            
    and CLIArgs =
        | [<CliPrefix(CliPrefix.None)>] Summary of ParseResults<SummaryArgs>
        | [<CliPrefix(CliPrefix.None)>] CreateMR of ParseResults<MRArgs>
        | [<CliPrefix(CliPrefix.None)>] CreateNewBranch of ParseResults<BranchArgs>
        interface IArgParserTemplate with
            member s.Usage =
                match s with 
                | Summary _ -> "Updates your README.md to current version"
                | CreateMR _ -> "Creates a new merge request"
                | CreateNewBranch _ -> "Creates a new branch"

        // add some more arguments such as selected content like only assay or study overview



