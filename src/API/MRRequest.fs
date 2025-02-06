namespace ARCSummary

open FSharp.Data

module MergeRequest =
    
    let baseAPIURL = "https://git.nfdi4plants.org/api/v4"

    let inline encodedBranchPath (pathOrId) = $"{baseAPIURL}/projects/{pathOrId}/repository/branches"
    let createNewBranch (personalAccessToken: string)  (pathOrId) (newBranchIdentifier : string) (refBranch : string) =
        Http.RequestString(
            url = (encodedBranchPath (pathOrId |> System.Uri.EscapeDataString)),
            httpMethod = "POST",
            headers = [ "PRIVATE-TOKEN", personalAccessToken ],
            body = HttpRequestBody.FormValues [
                "id", pathOrId
                "branch", newBranchIdentifier
                "ref", refBranch
            ]
        )

    let inline encodedAPIPath (pathOrId) = $"{baseAPIURL}/projects/{pathOrId}/merge_requests"

    let createMR (personalAccessToken: string)  (pathOrId) (newBranch) (main : string)  (commitTitle: string)=
        Http.RequestString(
            url = (encodedAPIPath (pathOrId |> System.Uri.EscapeDataString)),
            httpMethod = "POST",
            headers = [ "PRIVATE-TOKEN", personalAccessToken ],
            body = HttpRequestBody.FormValues [
                "id", pathOrId
                "source_branch", newBranch
                "target_branch", main
                "title", commitTitle
            ]
        )
    