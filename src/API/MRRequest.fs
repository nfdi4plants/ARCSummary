namespace ARCSummary

open FSharp.Data

module MergeRequest =
    
    let baseAPIURL = "https://git.nfdi4plants.org/api/v4"
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

    let inline encodedAPIPath (pathOrId) = $"{baseAPIURL}/projects/{pathOrId}/merge_requests"

    let createMR (pathOrId) (newBranch) (main : string)  (commitMessage : string)=
        Http.RequestString(
            url = (encodedAPIPath pathOrId),
            httpMethod = "POST",
            headers = [ "PRIVATE-TOKEN", personalAccessToken ],
            body = HttpRequestBody.FormValues [
                "id", pathOrId
                "source_branch", newBranch
                "target_branch", main
                "title", commitMessage
            ]
        )
    

