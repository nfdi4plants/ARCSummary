namespace ARCSummary.GitLabAPI

open FSharp.Data

type Branch =

    static member EncodeBranchPath (pathOrId : string, baseURL:string) =
        $"{baseURL}/projects/{pathOrId}/repository/branches"            

    static member CreateNewBranch (token : string, pathOrId: string, newBranch:string, refBranch:string, ?apiAddress:string) =
        let apiAddress = Option.defaultValue Common.dataHUBAPI apiAddress
        Http.Request(
            url = Branch.EncodeBranchPath(pathOrId |> System.Uri.EscapeDataString,apiAddress),
            httpMethod = "POST",
            headers = [ "PRIVATE-TOKEN", token ],
            body = HttpRequestBody.FormValues [
                "id", pathOrId
                "branch", newBranch
                "ref", refBranch
            ],
            silentHttpErrors = true
        )