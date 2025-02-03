namespace ARCSummary

open ARCtrl
open Argu
open prototypeCLI
open READMEAutomation
open MergeRequest

module CLI =

    [<EntryPoint>]
    let main args =
        let parser = ArgumentParser.Create<CLIArguments>(programName = "ARCSummary")
        try
            let res = parser.ParseCommandLine(args)
            
            match res.TryGetResult ARC_Directory, res.TryGetResult CreateMR with
            // check if a valid directory has been found
            | Some arcPath, None  ->
                match ARC.load(arcPath).ISA with
                //further check if investigation can be accessed and used to append the markdown
                | Some investigation ->
                    updateREADME arcPath investigation |> ignore
                    printfn "README.md updated successfully at %s" arcPath
                    0 
                | None ->
                    printfn "Failed to load investigation from ARC at %s" arcPath
                    1 
            | None, Some (pathOrId, newBranch, mainBranch, commitMessage) ->
                printfn "Creating Merge Request for %s..." pathOrId
                let response = createMR pathOrId newBranch mainBranch commitMessage
                printfn "Response: %s" response
                0
            | None, None -> 
                printfn "Invalid arguments.\n\n%s" (parser.PrintUsage())
                1
            | Some _, Some _ -> // prevent from running simultaneously
                printfn "Error: You cannot use both ARC_Directory and CreateMR commands simultaneously."
                1                
        with
        | :? ArguParseException as errorMessage ->
            eprintfn "Error parsing arguments: %s" errorMessage.Message
            1 
        | ex ->
            eprintfn "Unexpected error: %s" ex.Message
            1 





// https://docs.gitlab.com/ee/ci/
// github pull request main branch
// gitlab pull request main branch


// Dockerfile Version 1.1

// FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base

// FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
// ARG BUILD_CONFIGURATION=Release
// WORKDIR /src
// COPY "src" .
// RUN dotnet restore "./CLI/CLI.fsproj"
// RUN dotnet build "./CLI/CLI.fsproj" -c $BUILD_CONFIGURATION -o /build

// FROM build AS publish
// ARG BUILD_CONFIGURATION=Release
// RUN dotnet publish "./CLI/CLI.fsproj" -c $BUILD_CONFIGURATION -o /publish

// FROM base AS final

// COPY --from=publish /publish ./tool


// #ENTRYPOINT ["./tool"]

// Dockerfile Version 1.2

// FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base

// FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
// ARG BUILD_CONFIGURATION=Release
// WORKDIR /src
// COPY "src" .
// RUN dotnet restore "./CLI/CLI.fsproj"
// RUN dotnet build "./CLI/CLI.fsproj" -c $BUILD_CONFIGURATION -o /build

// FROM build AS publish
// ARG BUILD_CONFIGURATION=Release
// RUN dotnet publish "./CLI/CLI.fsproj" -c $BUILD_CONFIGURATION -o /publish

// FROM base AS final
// WORKDIR /tool 
// COPY --from=publish /publish .

// ENTRYPOINT ["./CLI"]  

// Dockerfile Version 1.3
// FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
// ARG BUILD_CONFIGURATION=Release
// WORKDIR /src
// COPY "src" .
// RUN dotnet restore "./CLI/CLI.fsproj"
// RUN dotnet build "./CLI/CLI.fsproj" -c $BUILD_CONFIGURATION -o /build

// FROM build AS publish
// RUN dotnet publish "./CLI/CLI.fsproj" -c $BUILD_CONFIGURATION -o /publish

// FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
// WORKDIR /tool
// COPY --from=publish /publish .
// RUN chmod +x /tool/CLI
// ENTRYPOINT ["/tool/CLI"]
