namespace ARCSummary

open FSharp.Data

module MergeRequest =
    

    type APIRequest =
        static member baseAPIURL = "https://git.nfdi4plants.org/api/v4"
        // let testAPIURL = "https://gitdev.nfdi4plants.org/api/v4"


        static member EncodeBranchPath (pathOrId, baseURL:string) =
            $"{baseURL}/projects/{pathOrId}/repository/branches"            

        // consider adding as static member
        static member CreateNewBranch (personalAccessToken: string, pathOrId, newBranchIdentifier:string, refBranch:string, ?apiAddress:string) =
            let apiAddress = Option.defaultValue APIRequest.baseAPIURL apiAddress
            Http.RequestString(
                url = (APIRequest.EncodeBranchPath((pathOrId |> System.Uri.EscapeDataString),apiAddress )), //?affirmation = affirmation
                httpMethod = "POST",
                headers = [ "PRIVATE-TOKEN", personalAccessToken ],
                body = HttpRequestBody.FormValues [
                    "id", pathOrId
                    "branch", newBranchIdentifier
                    "ref", refBranch
                ]
            )

        static member EncodeMRPath (pathOrId, baseURL:string) =
                $"{baseURL}/projects/{pathOrId}/merge_requests"       

        static member CreateMR (personalAccessToken: string, pathOrId, newBranch, main:string, commitTitle:string, ?apiAddress:string)=
            let apiAddress = Option.defaultValue APIRequest.baseAPIURL apiAddress
            Http.RequestString(
                url = (APIRequest.EncodeMRPath((pathOrId |> System.Uri.EscapeDataString), apiAddress)),
                httpMethod = "POST",
                headers = [ "PRIVATE-TOKEN", personalAccessToken ],
                body = HttpRequestBody.FormValues [
                    "id", pathOrId
                    "source_branch", newBranch
                    "target_branch", main
                    "title", commitTitle
                ]
            )
    

    // let isTestServer (affirmation:string) :bool = 
    //     affirmation = "yes"  

    // let getAPIURL (affirmation:string)  = 
    //     if isTestServer affirmation = true then testAPIURL else baseAPIURL

    // getAPIURL "yes" 

        // let inline encodedBranchPath (pathOrId) (affirmation:string) = 
        // $"{getAPIURL affirmation}/projects/{pathOrId}/repository/branches"