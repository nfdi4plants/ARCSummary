namespace ARCSummary 


open ARCtrl
open System
open System.Text
open ArcTable
open ArcQuerying
open Template
open ARCInstances

module PromptHelper = 


    let getOntologyForPrompt (table:ArcTable)  =
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
        String.Join("\n", tabChars, tabParams, tabFacs)


    let getBasicStudyPrompt (investigation:ArcInvestigation) = 
        investigation.Studies
        |> Seq.filter (fun (s:ArcStudy) -> s.TableCount <> 0)
        |> Seq.map(fun (study:ArcStudy) ->
            study
            |> Seq.map(fun (table:ArcTable) ->  
                let ontologies = getOntologyForPrompt table
                if not (String.IsNullOrWhiteSpace(ontologies)) then 
                    let studyID = $"## Study_{study.Identifier}_{table.Name} \n"
                    String.Join("\n", studyID, ontologies)
                else ""
                )
            |> Seq.filter(fun s -> not (String.IsNullOrWhiteSpace(s)))
            |> String.concat "\n"
        )
        |> String.concat "\n"

    let getEnhancedStudyPrompt (investigation:ArcInvestigation) = 
        investigation.Studies
        |> Seq.filter (fun (s:ArcStudy) -> s.TableCount <> 0)
        |> Seq.map(fun (study:ArcStudy) ->
            let basePrompt =
                study
                |> Seq.map(fun (table:ArcTable) -> 
                    let ontologies = getOntologyForPrompt table
                    if not (String.IsNullOrWhiteSpace(ontologies)) then 
                        let studyID = $"\n ## Study_{study.Identifier}_{table.Name}"
                        String.Join("\n", studyID, ontologies)
                    else ""
                    )
                |> Seq.filter(fun s -> not (String.IsNullOrWhiteSpace(s)))
                |> String.concat "\n"
            let studyDescription = 
                    if study.Description.IsSome then 
                        $"### Description {study.Identifier}: {study.Description.Value} \n"
                    else ""
            let addDetails = 
                    $"### Additional Details {study.Identifier} \n {createStudyAdditionalDetails (getStudyOverview investigation study)}"
            String.Join("\n", basePrompt, studyDescription, addDetails)
        )
        |> String.concat "\n"


    let getBasicAssayPrompt (investigation:ArcInvestigation) = 
        investigation.Assays
        |> Seq.filter (fun (a:ArcAssay) -> a.TableCount <> 0)
        |> Seq.map(fun (assay:ArcAssay) ->
            assay
            |> Seq.map(fun (table:ArcTable) ->  
                let ontologies = getOntologyForPrompt table
                if not (String.IsNullOrWhiteSpace(ontologies)) then 
                    let assayID = $"## Assay_{assay.Identifier}_{table.Name} \n"
                    String.Join("\n", assayID, ontologies)
                else ""
                )
            |> Seq.filter(fun s -> not (String.IsNullOrWhiteSpace(s)))
            |> String.concat "\n"
        )
        |> String.concat "\n"

    let getEnhancedAssayPrompt (investigation:ArcInvestigation) = 
        investigation.Assays
        |> Seq.filter (fun (a:ArcAssay) -> a.TableCount <> 0)
        |> Seq.map(fun (assay:ArcAssay) ->

            let basePrompt =
                assay
                |> Seq.map(fun (table:ArcTable) -> 
                    let ontologies = getOntologyForPrompt table
                    if not (String.IsNullOrWhiteSpace(ontologies)) then 
                        let assayID = $"\n ## Assay_{assay.Identifier}_{table.Name}"
                        String.Join("\n", assayID, ontologies)
                    else ""
                    )
                |> Seq.filter(fun s -> not (String.IsNullOrWhiteSpace(s)))
                |> String.concat "\n"
            let assayDescription = 
                    if assay.Description.IsSome then 
                        $"### Description {assay.Identifier}: {assay.Description.Value} \n"
                    else ""
            let addDetails = 
                    $"### Additional Details {assay.Identifier} \n {createAssayAdditionalDetails (getAssayOverview investigation assay)}"
            String.Join("\n", basePrompt, assayDescription, addDetails)
        )
        |> String.concat "\n"

open PromptHelper
module Prompt =
    let basicPrompt (investigation:ArcInvestigation) = 
        let sb = StringBuilder()
        sb.AppendLine("Summarize the biological and experimental objectives of the studies and assays below. Extract the key experimental factors and their values from the metadata in order to construct a 2-3 sentence summary of the objectives.") |> ignore
        sb.AppendLine($"The found studies are listed with their respective id consisting of Study_Identifier_TableName alongside the ontology information here: {getBasicStudyPrompt investigation}") |> ignore
        sb.AppendLine($"The found assay are listed respective table name alongside the ontology information here: {getBasicAssayPrompt investigation} ") |> ignore
        sb.AppendLine("Summarize in a short paragraph suitable for a methods section.")

    let enhancedPrompt (investigation:ArcInvestigation) =
        let sb = StringBuilder()
        sb.AppendLine("Summarize the biological and experimental objectives of the studies and assays below. Extract the key experimental factors and their values from the metadata in order to construct a 2-3 sentence summary of the objectives.") |> ignore
        sb.AppendLine($"The found studies are listed with their respective id consisting of Study_Identifier_TableName alongside the ontology information here: {getEnhancedStudyPrompt investigation}") |> ignore
        sb.AppendLine($"This workflow highlights with --> which StudyIDs have matching output to input of another TableID: \n {allStudyIDNodes (getAllTableNodes investigation)}") |> ignore
        sb.AppendLine($"The found assay are listed respective table name alongside the ontology information here: {getEnhancedAssayPrompt investigation} ") |> ignore
        sb.AppendLine($"This workflow highlights with --> which AssayIDs have matching output to input of another TableID: \n {allAssayIDNodes (getAllTableNodes investigation)}") |> ignore
        sb.AppendLine("Summarize in a short paragraph suitable for a methods section.")
    
    
    let hybridPrompt(investigation:ArcInvestigation) = 
        let sb = StringBuilder()
        sb.AppendLine($"The following overview table contains metadata of the entire investigation: {createOverviewTable (getTopLevelMetadata investigation)}") |> ignore
        sb.AppendLine($"The found studies are listed with their respective id consisting of Study_Identifier_TableName alongside the ontology information here: {getEnhancedStudyPrompt investigation}") |> ignore
        sb.AppendLine($"This workflow highlights with --> which StudyIDs have matching output to input of another TableID: \n {allStudyIDNodes (getAllTableNodes investigation)} \n") |> ignore
        sb.AppendLine($"The found assays are listed with their respective id consisting of Assay_Identifier_TableName alongside the ontology information here: {getEnhancedAssayPrompt investigation} ") |> ignore // not uniform to studys
        sb.AppendLine($"This workflow highlights with --> which AssayIDs have matching output to input of another TableID: \n {allAssayIDNodes (getAllTableNodes investigation)} \n") |> ignore
    
        sb.AppendLine("## INSTRUCTIONS (CHAIN-OF-THOUGHT)") |> ignore
        sb.AppendLine("1. **Identify the objectives based on key variables such as factors of the study**") |> ignore
        sb.AppendLine("2. **Infer the experimental design**: How are different conditions being compared?") |> ignore
        sb.AppendLine("3. **Map the experimental workflow** using the `-->` structure from Study to Assay.") |> ignore
        sb.AppendLine("4. **Summarize key methods used** in harvesting, extraction, and measurement stages.") |> ignore
        sb.AppendLine("5. Combine all insights into a cohesive summary structured in two paragraphs:") |> ignore
        sb.AppendLine("- **Paragraph 1 (2-3 sentences)**: State the main experimental objectives from 1.") |> ignore 
        sb.AppendLine("- **Paragraph 2 (5-7 sentences)**: Describe the experimental design and methodology in appropriate scientific language") |> ignore
        sb.AppendLine("6. Use any domain-specific vocabulary from the Description fields to improve specificity if available. \n") |> ignore
    
        sb.AppendLine("## CONSTRAINTS") |> ignore
        sb.AppendLine("- Use formal scientific language appropriate for the methods section or abstract.") |> ignore
        sb.AppendLine("- Keep the output concise but specific.") |> ignore
        sb.AppendLine("- Emphasize treatment factors, organism studied, and downstream assays.") |> ignore
        sb.AppendLine("- Avoid simply listing assays or instruments unless relevant to the objective or method.") |> ignore
        sb.AppendLine("- Weight content as follows: **[IMPORTANT] Experimental objectives → [MEDIUM] Methodology → [LESS] Equipment details**.") 