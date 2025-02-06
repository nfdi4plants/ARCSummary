# ARCSummary

>Create or update markdown content of ARCs dynamically

ARCSummary aims to facilitate the dynamic generation of markdown content. It serves as a up-to-date overview of the ARCs current state since all the information is derived from the content from an already established or starting investigation.

## Setup
Clone this repository locally and run with or without docker. 

### ARCsummary currently supports three main subcommands:

- **summary**     Updates your README.md to the current version
- **createmr**    Creates a new merge request
- **createnewbranch**   Creates a new branch


### Run via App
```bash
cd /path/to/your/CLI
dotnet restore
dotnet build
dotnet run summary /path/to/your/arc
dotnet run createmr idorurl updatedBranch main UpdatedREADME
dotnet run createnewbranch idorurl updatedBranch main
```

### Run via Docker environment
For reference as this is based on the docker setup provided by arc-export see the [docs](https://github.com/nfdi4plants/arc-export)
```bash
docker build -t dockerimage:latest /path/to/ARCSummary 
docker run -v "/path/to/your/arc:/arc" dockerimage:latest summary --arc-directory /arc
docker run arc-summary:latest createmr --token personalAccessToken --pathorid idorurl --sourcebranch updatedBranch --mainbranch main --committitle UpdatedREADME
docker run arc-summary:latest createnewbranch --token personalAccessToken --pathorid  idorurl --newbranch updatedBranch --mainbranch main 
```


## Help 
### For Summary:
```bash
USAGE: ARCSummary summary [--help] --arc-directory <arcPath>

OPTIONS:

    --arc-directory <arcPath>
                          Specify your ARC directory
    --help                display this list of options.
```

### For CreateNewBranch:
```bash
USAGE: ARCSummary createnewbranch [--help] --token <string> --pathorid <string>
                                  --newbranch <string> --mainbranch <string>

OPTIONS:

    --token <string>      Personal access token for gitlab
    --pathorid <string>   ID or URL-encdoded path of the project after .org/
    --newbranch <string>  Name of the new branch
    --mainbranch <string> Name of the target branch usally main
    --help                display this list of options.
```

### For CreateMR:
```bash
USAGE: ARCSummary createmr [--help] --token <string> --pathorid <string>
                           --sourcebranch <string> --mainbranch <string>
                           --committitle <string>

OPTIONS:

    --token <string>      Personal access token for gitlab
    --pathorid <string>   ID or URL-encdoded path of the project after .org/
    --sourcebranch <string>
                          Name of the source branch
    --mainbranch <string> Name of the target branch
    --committitle <string>
                          Title of the MR
    --help                display this list of options.
```