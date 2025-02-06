namespace ARCSummary


open Argu


module mainCLI =
    
    type SummaryArgs =
        | [<Mandatory>] ARC_Directory of arcPath : string 
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | ARC_Directory  _ -> "Specify your ARC directory" 

    and MRArgs = 
        | [<Mandatory>] Token of string
        | [<Mandatory>] PathOrId of string 
        | [<Mandatory>] SourceBranch of string 
        | [<Mandatory>] MainBranch of string
        | [<Mandatory>] CommitTitle of string
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Token _ -> "Personal access token for gitlab"
                | PathOrId _ -> "ID or URL-encdoded path of the project after .org/"
                | SourceBranch _ -> "Name of the source branch"
                | MainBranch _ -> "Name of the target branch"
                | CommitTitle _ -> "Title of the MR"
                // consider adding description as optional parameter
    
    and BranchArgs =
        | [<Mandatory>] Token of string
        | [<Mandatory>] PathOrId of string 
        | [<Mandatory>] NewBranch of string 
        | [<Mandatory>] MainBranch of string 
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Token _ -> "Personal access token for gitlab"
                | PathOrId _ -> "ID or URL-encdoded path of the project after .org/"
                | NewBranch _ -> "Name of the new branch"
                | MainBranch _ -> "Name of the target branch usally main"
                            
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



