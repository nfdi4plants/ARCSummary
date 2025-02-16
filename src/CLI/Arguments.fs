namespace ARCSummary


open Argu


module mainCLI =
    
    type SummaryArgs =
        | [<Mandatory>] [<AltCommandLine("-ap")>] ARC_Directory of arcPath : string 
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | ARC_Directory  _ -> "Specify your ARC directory" 

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

    and CommitArgs =
        | [<Mandatory>] [<AltCommandLine("-t")>] Token of string
        | [<Mandatory>] [<AltCommandLine("-pi")>] PathOrId of string 
        | [<Mandatory>] [<AltCommandLine("-nb")>] NewBranch of string 
        | [<Mandatory>] [<AltCommandLine("-mb")>] MainBranch of string 
        | [<Mandatory>] [<AltCommandLine("-ct")>] CommitTitle of string
        | [<Mandatory>] [<AltCommandLine("-cm")>] CommitMessage of string
        | [<Mandatory>] [<AltCommandLine("-fp")>] FilePath of string 
        | [<EqualsAssignment>] [<AltCommandLine("-aa")>] APIAdress of string
        | [<EqualsAssignment>] [<AltCommandLine("-ue")>] UserEmail of string // [<Mandatory>]
        | [<EqualsAssignment>] [<AltCommandLine("-un")>] UserName of string // [<Mandatory>]
        | [<EqualsAssignment>] [<AltCommandLine("-f")>] Force of bool 
        | [<EqualsAssignment>] [<AltCommandLine("-m")>] Message of string
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Token _ -> "Personal access token for gitlab"
                | PathOrId _ -> "ID or URL-encdoded path of the project after .org/"
                | NewBranch _ -> "Name of the new branch"
                | MainBranch _ -> "Name of the target branch usally main"
                | CommitTitle _ -> "Title of the MR"
                | CommitMessage _ -> "Commit message"      
                | FilePath _ -> "File Path to your updated README.md"          
                | APIAdress _ -> "Testing Server URL"                
                | UserEmail _ -> "Specifiy your User Email"
                | UserName _ -> "Specifiy your Username"
                | Force _ -> "When true overwrites the target branch with a new commit based on the start_branch or start_sha"
                | Message _ -> "Add a Commit Message"

    and CLIArgs =
        | [<CliPrefix(CliPrefix.None)>] Summary of ParseResults<SummaryArgs>
        | [<CliPrefix(CliPrefix.None)>] CreateNewBranch of ParseResults<BranchArgs>       
        | [<CliPrefix(CliPrefix.None)>] CreateMR of ParseResults<MRArgs>
        | [<CliPrefix(CliPrefix.None)>] CreateCommitWorkflow of ParseResults<CommitArgs>         

        interface IArgParserTemplate with
            member s.Usage =
                match s with 
                | Summary _ -> "Updates your README.md to current version"
                | CreateNewBranch _ -> "Creates a new branch"    
                | CreateMR _ -> "Creates a new merge request"
                | CreateCommitWorkflow _ -> "Creates a new Branch, Creates a Commit with changed Files, Creates MR"   




        // add some more arguments such as selected content like only assay or study overview


