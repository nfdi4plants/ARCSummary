# ARCSummary

>Library to create or update markdown content of ARCs dynamically
ARCSummary aims to facilitate the dynamic generation of markdown content, either as a base or adding to the already established markdown documentation of your investigation.
It serves as a up-to-date overview of the ARCs current state since all the information is derived from the content from an already established or starting project.

## Local Setup
### Run via App
```bash
cd /path/to/your/CLI
dotnet restore
dotnet run --arc-directory /path/to/your/arc
```

### Run via Docker environment
```bash
docker build -t arc-export8:latest .   
docker run -v "/path/to/your/arc" arc-export8:latest --arc-directory /arc
```

This should result in one of the following messages:
*README.md updated successfully at /arc*
or
*File is already updated
README.md updated successfully at /arc*

## Installation
Clone this repository locally and run with or without docker. Publication on NuGet will follow.

### Dependencies + Versions

Project 'Core' has the following package references [net8.0]: 

|Top-level Package   |    Requested  | Resolved|
| ------------------ | ------------- | --------|
|  ARCtrl            |    2.2.4      | 2.2.4   |
|  ARCtrl.NET        |    2.0.2      | 2.0.2   |
|  Argu              |    6.2.4      | 6.2.4   |
|  FSharp.Core       |    9.0.100    | 9.0.100 |

Project 'CLI' has the following package references [net8.0]: 

|Top-level Package   |    Requested  | Resolved|
| ------------------ | ------------- | --------|
|  ARCtrl            |    2.3.1      | 2.3.1   |
|  Argu              |    6.2.4      | 6.2.4   |
|  FSharp.Core       |    9.0.100    | 9.0.100 |
