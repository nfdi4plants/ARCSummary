namespace ARCSummary

open FSharp.Data
open JsonDSL

module MergeRequest =
    

    let baseAPIURL = "https://git.nfdi4plants.org/api/v4"
        // let testAPIURL = "https://gitdev.nfdi4plants.org/api/v4"    

    type APIRequest =

        static member EncodeBranchPath (pathOrId : string, baseURL:string) =
            $"{baseURL}/projects/{pathOrId}/repository/branches"            

        static member CreateNewBranch (personalAccessToken: string, pathOrId: string, newBranchIdentifier:string, refBranch:string, ?apiAddress:string) =
            let apiAddress = Option.defaultValue baseAPIURL apiAddress
            Http.RequestString(
                url = (APIRequest.EncodeBranchPath((pathOrId |> System.Uri.EscapeDataString),apiAddress )),
                httpMethod = "POST",
                headers = [ "PRIVATE-TOKEN", personalAccessToken ],
                body = HttpRequestBody.FormValues [
                    "id", pathOrId
                    "branch", newBranchIdentifier
                    "ref", refBranch
                ]
            )

        static member EncodeMRPath (pathOrId : string, baseURL:string) =
            $"{baseURL}/projects/{pathOrId}/merge_requests"       

        static member CreateMR (personalAccessToken: string, pathOrId :string , newBranch:string, main:string, commitTitle:string, ?apiAddress:string)=
            let apiAddress = Option.defaultValue baseAPIURL apiAddress
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
    

    // //Implement Create or Delete and other action types
    // type ActionType =
    //     | Create
    //     | Update
    //     | Delete

    type WorkflowRequest =

        // static member ActionTypeToJObject (a : ActionType) =
        //     match a with
        //     | Create -> "create"
        //     | Update -> "update"
        //     | Delete -> "delete"        

    
        static member CreateAction  (filePath:string) (content:string) = //(action: ActionType) 
            object {
                property "action" "update"
                property "file_path" filePath  // should always be "README.md"
                property "content" content // markdown
            }            
        

        static member EncodeFilePath (pathOrId, baseURL:string) =
            let encodedPath = System.Uri.EscapeDataString "README.md"
            $"{baseURL}/projects/{System.Uri.EscapeDataString pathOrId}/repository/files/{encodedPath}/raw?ref=main"
        static member GetMarkdownContent (personalAccessToken:string, pathOrId:string, mainBranch:string, ?apiAddress:string)= // add token
            let apiAddress = Option.defaultValue baseAPIURL apiAddress
            Http.RequestString(
                url = (WorkflowRequest.EncodeFilePath((pathOrId |> System.Uri.EscapeDataString), apiAddress)),
                httpMethod = "GET",
                headers = [ "PRIVATE-TOKEN", personalAccessToken ] //,
                // body = HttpRequestBody.FormValues [
                //     "id", pathOrId 
                //     "file_path", "README.md"
                //     "ref", mainBranch
                // ]
          )



        static member EncodeCommit (pathOrId, baseURL:string) =
            $"{baseURL}/projects/{pathOrId}/repository/commits"

    
        static member CreateCommit (pathOrId:string, main:string, newBranch:string, commitMessage:string, filePath:string, content:string, ?userEmail:string, ?userName:string, ?force:bool, ?apiAddress:string)=
            let apiAddress = Option.defaultValue baseAPIURL apiAddress
            let actions = WorkflowRequest.CreateAction (filePath |> System.Uri.EscapeDataString)
            let requestBody =
                object {
                    property "id" pathOrId
                    property "branch" main
                    property "start_branch" newBranch
                    property "commit_message" commitMessage
                    property "actions" actions
                    property "content" content
                    property "author_email" (-. userEmail)
                    property "author_name" (-. userName)
                    property "force" (-. force)                
                }              

            Http.RequestString(
                url = (WorkflowRequest.EncodeCommit((pathOrId |> System.Uri.EscapeDataString), apiAddress)),
                httpMethod = "POST",
                headers = [ "Content-Type", "application/json" ],
                body = HttpRequestBody.TextRequest (requestBody.ToJsonString())
            )        






    // let isTestServer (affirmation:string) :bool = 
    //     affirmation = "yes"  

    // let getAPIURL (affirmation:string)  = 
    //     if isTestServer affirmation = true then testAPIURL else baseAPIURL

    // getAPIURL "yes" 

        // let inline encodedBranchPath (pathOrId) (affirmation:string) = 
        // $"{getAPIURL affirmation}/projects/{pathOrId}/repository/branches"