#r "nuget: FSharp.Data, 6.4.1"

open FSharp.Data 

let baseAPIURL = "https://git.nfdi4plants.org/api/v4"
let projectPathEncoded = "scholz/SFB175AraCore" |> System.Uri.EscapeDataString
let personalAccessToken = "Xk1tuPqKhx4fti-nMWEh"

//let s = Http.RequestString(url = nfdi4plantsURL) // , query = [ "test", "foo" ], httpMethod = "GET"

// let urlCommits = "https://git.nfdi4plants.org/api/V4/projects/scholz%SFB175AraCore/repository/commits"
// let url = $"https://git.nfdi4plants.org/api/V4/projects?private_token={personalAccessToken}"

// Http.Request(
//     url,
//     httpMethod = "GET"
// )

let commitsEndpoint = $"{baseAPIURL}/projects/{projectPathEncoded}/repository/commits"

Http.RequestString(
    url = commitsEndpoint,
    headers = [ "PRIVATE-TOKEN", personalAccessToken ] 
)

let inline encodedAPIPath (pathOrId) = $"{baseAPIURL}/projects/{pathOrId}/merge_requests"

let createMR (pathOrId) (main) (newBranch) (commitMessage)=
    Http.RequestString(
        url = (encodedAPIPath pathOrId),
        httpMethod = "POST",
        headers = [ "PRIVATE-TOKEN", personalAccessToken ],
        body = HttpRequestBody.FormValues [
            "id", pathOrId
            "source_branch", main
            "target_branch", newBranch
            "title", commitMessage
        ]
    )


// let newAPIPath = "scholz/api-testing-arcsummary" |> System.Uri.EscapeDataString

// let newEp = $"{baseAPIURL}/projects/{newAPIPath}/repository/commmits"

// Http.RequestString(
//     newEp

// )