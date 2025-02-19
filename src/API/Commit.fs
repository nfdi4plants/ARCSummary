namespace ARCSummary.GitLabAPI

open FSharp.Data
open JsonDSL

type ActionType =
    | Create
    | Update
    | Delete

    static member actionTypeTOJObject (a : ActionType) =
        match a with
        | Create -> "create"
        | Update -> "update"
        | Delete -> "delete"

type CommitAction = {
        Action: ActionType
        File_path: string
        Content: string
    }

    with

    static member create (action: ActionType) (file_path: string) (content: string) =
        { Action = action; File_path = file_path; Content = content }

    static member commitActionToJObject (o : CommitAction) = 
        object {
            property "action" (ActionType.actionTypeTOJObject o.Action)
            property "file_path" (System.Uri.EscapeDataString o.File_path)
            property "content" o.Content
        }

type Commit =

        static member EncodeFilePath (pathOrId, baseURL:string) =
            let encodedPath = System.Uri.EscapeDataString "README.md"
            $"{baseURL}/projects/{System.Uri.EscapeDataString pathOrId}/repository/files/{encodedPath}/raw?ref=main"

        static member ListCommits (pathOrId:string, branch:string, token, ?apiAddress:string) = 
            let apiAddress = Option.defaultValue Common.dataHUBAPI apiAddress
            Http.RequestString(
                url = Commit.EncodeFilePath(pathOrId |> System.Uri.EscapeDataString, apiAddress),
                httpMethod = "GET",
                headers = [ "PRIVATE-TOKEN", token ]
          )


        static member EncodeCommit (pathOrId, baseURL:string) =
            $"{baseURL}/projects/{pathOrId}/repository/commits"

    
        static member CreateCommit (token: string, pathOrId:string, branch:string,  commitMessage:string, actions : CommitAction seq, ?startBranch:string, ?userEmail:string, ?userName:string, ?force:bool, ?apiAddress:string)=
            let apiAddress = Option.defaultValue Common.dataHUBAPI apiAddress
            let actions = array {
                for action in actions do
                    yield CommitAction.commitActionToJObject action
            }
            let requestBody =
                object {
                    property "id" pathOrId
                    property "branch" branch
                    property "start_branch" (-. startBranch)
                    property "commit_message" commitMessage
                    property "actions" actions
                    property "author_email" (-. userEmail)
                    property "author_name" (-. userName)
                    property "force" (-. force)                
                }              

            Http.Request(
                url = Commit.EncodeCommit(pathOrId |> System.Uri.EscapeDataString, apiAddress),
                httpMethod = "POST",
                headers = [ 
                    "PRIVATE-TOKEN", token; 
                    "Content-Type", "application/json" ],
                body = HttpRequestBody.TextRequest (requestBody.ToJsonString()),
                silentHttpErrors = true
            )       