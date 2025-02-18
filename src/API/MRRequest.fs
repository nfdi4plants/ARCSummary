namespace ARCSummary.GitLabAPI

open FSharp.Data
open JsonDSL

type MergeRequest =
    
    static member EncodeMRPath (pathOrId : string, baseURL:string) =
        $"{baseURL}/projects/{pathOrId}/merge_requests"       

    static member CreateMR (token: string, pathOrId :string , sourceBranch:string, targetBranch:string, title:string, ?apiAddress:string)=
        let apiAddress = Option.defaultValue Common.dataHUBAPI apiAddress
        Http.Request(
            url = (MergeRequest.EncodeMRPath((pathOrId |> System.Uri.EscapeDataString), apiAddress)),
            httpMethod = "POST",
            headers = [ "PRIVATE-TOKEN", token ],
            body = HttpRequestBody.FormValues [
                "id", pathOrId
                "source_branch", sourceBranch
                "target_branch", targetBranch
                "title", title
            ],
            silentHttpErrors = true
        )