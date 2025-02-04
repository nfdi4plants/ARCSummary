

// FSX for testing purposes only



#r "nuget: FSharp.Data, 6.4.1"

open FSharp.Data 
open System.IO

let baseAPIURL = "https://git.nfdi4plants.org/api/v4"
let projectPathEncoded = "scholz/SFB175AraCore" |> System.Uri.EscapeDataString
let personalAccessToken = "Xk1tuPqKhx4fti-nMWEh"

let inline encodedBranchPath (pathOrId) = $"{baseAPIURL}/projects/{pathOrId}/repository/branches" 
let createNewBranch (pathOrId) (newBranchIdentifier : string) (refBranch : string) =
    Http.RequestString(
        url = (encodedBranchPath pathOrId),
        httpMethod = "POST",
        headers = [ "PRIVATE-TOKEN", personalAccessToken ],
        body = HttpRequestBody.FormValues [
            "id", pathOrId
            "branch", newBranchIdentifier
            "ref", refBranch
        ]
    )
let newEP = "scholz/api-testing-arcsummary" |> System.Uri.EscapeDataString

createNewBranch  newEP"test2" "main"


let inline commitsEndpoint (pathOrId) = $"{baseAPIURL}/projects/{pathOrId}/repository/commits"

Http.RequestString(
    url = commitsEndpoint projectPathEncoded,
    httpMethod = "GET",
    headers = [ "PRIVATE-TOKEN", personalAccessToken ]
)

let newAPIPath = "scholz/api-testing-arcsummary" |> System.Uri.EscapeDataString
let newEp = commitsEndpoint newAPIPath

Http.RequestString(
    newEp ,
    httpMethod = "GET",
    headers = [ "PRIVATE-TOKEN", personalAccessToken ]
)

let myREADME = "README.md" |> System.Uri.EscapeDataString
let updatedAPIPath = $"{baseAPIURL}/projects/{projectPathEncoded}/repository/files/{myREADME}"
let updateMDFileAPI branch filePath =
    let content = File.ReadAllText(filePath)
    Http.RequestString(
        url = updatedAPIPath,
        httpMethod = "PUT",
        headers = [ "PRIVATE-TOKEN", personalAccessToken ],
        body = HttpRequestBody.FormValues [
            "branch", branch
            "content", content
        ]
    )

updateMDFileAPI "main" "/Users/olescholz/Desktop/ARC-Summary/README.md"

//First create manually a new Testbranch and check if merge request functions

let inline newMRPath (pathOrId) = $"{baseAPIURL}/projects/{pathOrId}/merge_requests"
let createMR (pathOrId) (main) (newBranch) (commitMessage) =
    Http.RequestString(
        url = newMRPath pathOrId,
        httpMethod = "POST",
        headers = [ "PRIVATE-TOKEN", personalAccessToken ],
        body = HttpRequestBody.FormValues [
            "id", pathOrId
            "source_branch", main
            "target_branch", newBranch
            "title", commitMessage
        ]
    )

createMR newAPIPath "Update_README" "main" "Update_README.md_Test_1"

// This part is up to the user:
// let fetchMRIID (pathOrId)  =
//     Http.RequestString(
//         url = $"{newMRPath pathOrId}",
//         httpMethod = "GET",
//         headers = [ "PRIVATE-TOKEN", personalAccessToken ]
//     )


// let mergeMR (pathOrId) (internalID) =
//     Http.RequestString(
//         url = $"/projects/{(pathOrId)}/merge_requests/{internalID}/merge",
//         httpMethod = "PUT",
//         body = HttpRequestBody.FormValues [
//             "id", pathOrId
//             "merge_request_iid", internalID
//         ]
//     )



// let newEps = $"{baseAPIURL}/projects"
// Http.RequestString(
//     url = newEps,
//     httpMethod = "GET",
//     headers = [ "PRIVATE-TOKEN", personalAccessToken ]
// )
// |> JsonValue.Parse
// |> _.AsArray()
// |> Array.map _.GetProperty("web_url")
// |> Array.map _.GetProperty("name")


//let s = Http.RequestString(url = nfdi4plantsURL) // , query = [ "test", "foo" ], httpMethod = "GET"

// let urlCommits = "https://git.nfdi4plants.org/api/V4/projects/scholz%SFB175AraCore/repository/commits"
// let url = $"https://git.nfdi4plants.org/api/V4/projects?private_token={personalAccessToken}"

// Http.Request(
//     url,
//     httpMethod = "GET"
// )

// Docs

// https://docs.gitlab.com/ee/api/rest/authentication.html
// https://docs.gitlab.com/ee/api/rest/

//let s = Http.RequestString(url = nfdi4plantsURL) // , query = [ "test", "foo" ], httpMethod = "GET"

// let urlCommits = "https://git.nfdi4plants.org/api/V4/projects/scholz%SFB175AraCore/repository/commits"
// let url = $"https://git.nfdi4plants.org/api/V4/projects?private_token={personalAccessToken}"

// Http.Request(
//     url,
//     httpMethod = "GET"
// )

// let commitsEndpoint = $"{baseAPIURL}/projects/{projectPathEncoded}/repository/commits"

// Http.RequestString(
//     url = commitsEndpoint,
//     headers = [ "PRIVATE-TOKEN", personalAccessToken ] 
// )

// let inline encodedAPIPath (pathOrId) = $"{baseAPIURL}/projects/{pathOrId}/merge_requests"

// let createMR (pathOrId) (main) (newBranch) (commitMessage)=
//     Http.RequestString(
//         url = (encodedAPIPath pathOrId),
//         httpMethod = "POST",
//         headers = [ "PRIVATE-TOKEN", personalAccessToken ],
//         body = HttpRequestBody.FormValues [
//             "id", pathOrId
//             "source_branch", main
//             "target_branch", newBranch
//             "title", commitMessage
//         ]
//     )


// let newAPIPath = "scholz/api-testing-arcsummary" |> System.Uri.EscapeDataString

// let newEp = $"{baseAPIURL}/projects/{newAPIPath}/repository/commmits"

// Http.RequestString(
//     newEp

// )