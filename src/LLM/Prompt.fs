namespace ARCSummary 


open ARCtrl
open System
open System.Text
open ArcTable
open ArcQuerying

module Prompt = 


    // Function after mapping on tables

    let getStudyPrompt (investigation:ArcInvestigation) =
        investigation.Studies
        |> Seq.filter (fun (s:ArcStudy) -> s.TableCount <> 0)
        |> Seq.map(fun (study:ArcStudy) ->
            study
            |> Seq.map(fun (table:ArcTable) -> 
                let studyID = $"## Study_{study.Identifier}_{table.Name} \n"
                let tabChars = 
                    if not (List.isEmpty (getAllCharacteristics table)) then
                        $"Characteristics: ({getAllOntologyInfos (getOntologyHeaders(getAllCharacteristics table)) table})"
                    else ""
                let tabParams = 
                    if not (List.isEmpty (getAllParameters table)) then
                        $"Parameters: ({getAllOntologyInfos (getOntologyHeaders(getAllParameters table)) table})"
                    else ""
                let tabFacs = 
                    if not (List.isEmpty (getAllFactors table)) then
                        $"Factors: {getAllOntologyInfos (getOntologyHeaders(getAllFactors table)) table})"
                    else ""
                String.Join("\n", studyID, tabChars, tabParams, tabFacs)
                )
            |> String.concat "\n"
        )
        |> String.concat "\n"

    let getAssayPrompt (investigation:ArcInvestigation) =
        investigation.Assays
        |> Seq.filter (fun (a:ArcAssay) -> a.TableCount <> 0)
        |> Seq.map(fun (assay:ArcAssay) ->
            assay
            |> Seq.map(fun (table:ArcTable) -> 
                let assayID = $"## Assay_{assay.Identifier}_{table.Name} "
                let tabChars = 
                    if not (List.isEmpty (getAllParameters table)) then
                        $"Characteristics: ({getAllOntologyInfos (getOntologyHeaders(getAllCharacteristics table)) table})"
                    else ""
                let tabParams = 
                    if not (List.isEmpty (getAllParameters table)) then
                        $"Parameters: ({getAllOntologyInfos (getOntologyHeaders(getAllParameters table)) table})"
                    else ""
                let tabFacs = 
                    if not (List.isEmpty (getAllFactors table)) then
                        $"Factors: {getAllOntologyInfos (getOntologyHeaders(getAllFactors table)) table})"
                    else ""
                String.Join("\n", assayID, tabChars, tabParams, tabFacs)
                )
            |> String.concat "\n"
        )
        |> String.concat "\n"


    let summaryPrompt (investigation:ArcInvestigation) = //include previous and suceeding tables or assays/studies
        let sb = StringBuilder()
        //refactor first line
        sb.AppendLine("Summarize the biological and experimental objectives of the studies and assays below. Extract the key experimental factors and their values from the metadata in order to construct a 2-3 sentence summary of the objectives.") |> ignore
        sb.AppendLine($"The found studies are listed with their respective id consisting of Study_Identifier_TableName alongside the ontology information here: {getStudyPrompt investigation}") |> ignore
        sb.AppendLine($"This workflow highlights with --> which StudyIDs have matching output to input of another TableID: \n {allStudyIDNodes (getAllTableNodes investigation)}") |> ignore
        sb.AppendLine($"The found assay are listed respective table name alongside the ontology information here: {getAssayPrompt investigation} ") |> ignore
        sb.AppendLine($"This workflow highlights with --> which AssayIDs have matching output to input of another TableID: \n {allAssayIDNodes (getAllTableNodes investigation)}") |> ignore
        sb.AppendLine("Summarize in a short paragraph suitable for a methods section.")

