# ARCSummary

>Create or update markdown content of ARCs dynamically

ARCSummary aims to facilitate the dynamic generation of markdown content. It serves as a up-to-date overview of the ARCs current state since all the information is derived from the content from an already established or starting investigation.

## Setup
Clone this repository locally and run with or without docker. 

### ARCsummary currently supports two main subcommands:

- **summary**     Updates your README.md to the current version
- **summarymr**    Pushes Updated Summary to side branch and opens a MergeRequest onto main branch.

### Run via App
```bash
cd /path/to/your/CLI
dotnet restore
dotnet build
dotnet run summary -d /path/to/your/arc
dotnet run summarymr -d /path/to/your/arc -t your_access_token -i user/repository
```

### Run via Docker environment
For reference as this is based on the docker setup provided by arc-export see the [docs](https://github.com/nfdi4plants/arc-export)
```bash
docker build -t dockerimage:latest /path/to/ARCSummary 
docker run -v "/path/to/your/arc:/arc" dockerimage:latest summary -d /arc
docker run -v "/path/to/your/arc:/arc" dockerimage:latest summarymr -d /arc -t your_access_token -i user/repository
```


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