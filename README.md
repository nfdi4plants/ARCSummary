# ARCSummary

>ARCSummary Library to create or update markdown content of ARCs dynamically


### Local Setup
## Run via App


## Run via Docker environment
docker build -t arc-export8:latest .   
docker run -v "/Users/pathofyourArc:/arc" arc-export8:latest --arc-directory /arc

This should result in one of the following messages:
README.md updated successfully at /arc
or
File is already updated
README.md updated successfully at /arc
## Release Notes / Documentation

### Dependencies + Versions
_dotnet list package_ Project 'Core' has the following package references [net9.0]: 

|Top-level Package   |    Requested  | Resolved|
| ------------------ | ------------- | --------|
|  ARCtrl            |    2.2.4      | 2.2.4   |
|  ARCtrl.NET        |    2.0.2      | 2.0.2   |
|  FSharp.Core       |    9.0.100    | 9.0.100 |
