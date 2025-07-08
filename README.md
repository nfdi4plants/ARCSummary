# ARCSummary

>Create or update markdown content of ARCs dynamically

ARCSummary aims to facilitate the dynamic generation of markdown content. It serves as a up-to-date overview of the ARCs current state since all the information is derived from the content of an already established or starting investigation.

## Setup
Clone this repository locally to run with or without docker. 

### ARCsummary currently supports two main subcommands:

- **summary**     Updates your README.md to the current version
- **summarymr**    Pushes the updated summary to a side branch and opens a MergeRequest onto the main branch.

### Download and run Docker image
```bash
docker pull ghcr.io/nfdi4plants/arc-summary:main

docker run -v "</path_to_your_arc>:/arc" ghcr.io/nfdi4plants/arc-summary:main summary -d /arc
docker run -v "</path_to_your_arc>:/arc" ghcr.io/nfdi4plants/arc-summary:main summarymr -d /arc -t your_access_token -i user/repository
```

### Build and run Docker image
For reference as this is based on the docker setup provided by arc-export see the [docs](https://github.com/nfdi4plants/arc-export)
```bash
docker build -t dockerimage:latest /path/to/ARCSummary 

docker run -v "</path_to_your_arc>:/arc" dockerimage:latest summary -d /arc
docker run -v "</path_to_your_arc>:/arc" dockerimage:latest summarymr -d /arc -t your_access_token -i user/repository
```

### Build and run .NET app
```bash
cd /path/to/your/CLI
dotnet restore
dotnet build
dotnet run summary -d /path/to/your/arc
dotnet run summarymr -d /path/to/your/arc -t your_access_token -i user/repository
```

### Update via YAML
Alternatively you can include an empty YML file at the following root of your arc ("../.arc/arc-summary.yml"), which will return the default order. A suitable theme can be chosen [here](https://github.com/nfdi4plants/ARCSummary/blob/main/src/Core/SummaryStyles.fs). For example for the publication style just include:
```bash
publicationstyle
```

Create a custom format using the currently supported format:
```bash
Custom:
   - Investigation # Top-level information based on metadata
      - Title
      - Description
      - Contacts
      - Publication
   - TOC # Table of Contents
   - ISAGraph # Flowchart comparing nodes between studies & assays
   - OverviewTable # Additional table based on metadata
   - Studies
      - Title
      - Description
      - Additionaldetails
      - Annotationheaders
   - Assays
      - Title
      - Description
      - Additionaldetails
      - Annotationheaders
```
The order and sections to include are at the user's discretion. All input is later interpreted in complete lowercase.


## Help 
### For Summary:
```bash
USAGE: ARCSummary summary [--help] --arc-directory <arcPath>

OPTIONS:

    --arc-directory, -d, -ap <arcPath>
                          Location of the ARC in the Filesystem
    --help                display this list of options.
```

### For SummaryMR:
```bash
USAGE: ARCSummary summarymr [--help] --arc-directory <string> --token <string> --pathorid <string> [--commitmessage <string>] [--mrtitle <string>] [--sourcebranch <string>] [--targetbranch <string>] [--username <string>] [--useremail <string>] [--apiadress <string>]

OPTIONS:

    --arc-directory, -d <string> 
        MANDATORY: Location of the ARC in the Filesystem

    --token, -t <string>  
        MANDATORY: Personal access token for gitlab

    --pathorid, -i <string> 
        MANDATORY: ID or URL-encdoded path of the project after .org/, e.g. username/myprojectname

    --commitmessage, --message <string>
        OPTIONAL: Message to be used for the commit.

    --mrtitle, --title <string>
        OPTIONAL: Title of the Merge Request

    --sourcebranch <string>
        OPTIONAL: Name of the branch to which the commit should be pushed, and which will be the
                          source branch of the MR. Default is `arc-summary`

    --targetbranch <string>
        OPTIONAL: Name of the reference branch which is the target for the MR. Default is `main`

    --username, --name <string>
        OPTIONAL: Username to be used for the commit

    --useremail, --email <string>
        OPTIONAL: Email to be used for the commit

    --apiadress, --api <string>
        OPTIONAL: Testing Server URL
        
    --help 
        display this list of options.
```

### Prompt generation

Currently ARCSummary supports the generation of three prompts: basic, enhanced and hybrid (enhanced + ToC) for use with ChatGPT

```bash
    basicprompt, -p1 <options>
        Generates a basic prompt based on arc-path for
        ai-asssisted paragraph on the investigations objectives
        
    enhancedprompt, -p2 <options>
        Generates a enhanced prompt based on arc-path for
        ai-asssisted paragraph on the investigations objectives

    hybridprompt, -p3 <options>
        Generates a enhanced + CoT prompt based on arc-path
        for ai-asssisted paragraph on the investigations objectives
```