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
        sb.AppendLine("
        Summarize the biological and experimental objectives of the following Investigation based on the available metadata of its Studies and Assays.
        You will receive identifiers, ontology annotations, and descriptive metadata for each component.
        Extract relevant experimental factors and variables to construct a structured summary in two paragraphs:
        - **Paragraph 1 (2-3 sentences):** Summarize the experimental objective of the Investigation
        - **Paragraph 2 (5-7 sentences):** Describe the experimental design and methodology
        Employ formal scientific language appropriate for an abstract and methods section.") |> ignore
        if not investigation.Description.IsNone then
            sb.AppendLine($"### Investigation Description: \n {investigation.Description.Value}") |> ignore
        sb.AppendLine($"### Top-Level-Metadata: \n {createOverviewTable (getTopLevelMetadata investigation)}") |> ignore
        sb.AppendLine($"{getEnhancedStudyPrompt investigation}") |> ignore
        sb.AppendLine($"{getEnhancedAssayPrompt investigation}") 


    let enhancedPrompt (investigation:ArcInvestigation) =
        let sb = StringBuilder()
        sb.AppendLine("
        Summarize the biological and experimental objectives of the following Investigation based on the available metadata of its Studies and Assays.
        You will receive identifiers, ontology annotations, and descriptive metadata for each component.
        Extract relevant experimental factors and variables to construct a structured summary in two paragraphs:
        - **Paragraph 1 (2-3 sentences):** Summarize the experimental objective of the Investigation
        - **Paragraph 2 (5-7 sentences):** Describe the experimental design and methodology
        Employ formal scientific language appropriate for an abstract and methods section.") |> ignore
        if not investigation.Description.IsNone then
            sb.AppendLine($"### Investigation Description: \n {investigation.Description.Value}") |> ignore
        sb.AppendLine($"### Top-Level-Metadata: \n {createOverviewTable (getTopLevelMetadata investigation)}") |> ignore
        sb.AppendLine($"{getEnhancedStudyPrompt investigation}") |> ignore
        sb.AppendLine($"### Study Table Associations (indicate with --> an input/output link): \n {allStudyIDNodes (getAllTableNodes investigation)}") |> ignore
        sb.AppendLine($"{getEnhancedAssayPrompt investigation}") |> ignore
        sb.AppendLine($"### Assay Table Associations (indicate with --> an input/output link): \n {allAssayIDNodes (getAllTableNodes investigation)}") 



    let hybridPrompt(investigation:ArcInvestigation) = 
        let sb = StringBuilder()
        if not investigation.Description.IsNone then
            sb.AppendLine($"### Investigation Description: \n {investigation.Description.Value}") |> ignore
        sb.AppendLine($"### Top-Level-Metadata: \n {createOverviewTable (getTopLevelMetadata investigation)}") |> ignore
        sb.AppendLine($"{getEnhancedStudyPrompt investigation}") |> ignore
        sb.AppendLine($"### Study Table Associations (indicate with --> an input/output link): \n {allStudyIDNodes (getAllTableNodes investigation)} \n") |> ignore
        sb.AppendLine($"{getEnhancedAssayPrompt investigation} ") |> ignore 
        sb.AppendLine($"### Assay Table Associations (indicate with --> an input/output link): \n {allAssayIDNodes (getAllTableNodes investigation)} \n") |> ignore

        sb.AppendLine("
        ### Instructions 
        1. Identify the objectives based on key variables such as factors of the study
        2. Infer the experimental design: How are different conditions being compared?
        3. Account for the relationships between Studies and Assays based on the association of tables.
        4. Summarize key methods used in harvesting, extraction, and measurement stages.
        5. Use domain-specific vocabulary from the Description fields if available.
        6. Combine all insights into a cohesive summary structured in two paragraphs:
            - **Paragraph 1 (2-3 sentences)**: State the main experimental objectives of the Investigation
            - **Paragraph 2 (5-7 sentences)**: Describe the experimental design and methodology
        
        ### Constraints
            - Use formal scientific language appropriate for an abstract and methods section.
            - Pay attention to relevance, coherence, factual consistency and fluency.
            - Emphasize treatment factors, organism, and downstream assays.
            - Avoid simply listing assays or instruments unless relevant to the objective or method.
            - Weight content as follows: **[IMPORTANT] Experimental objectives → [MEDIUM] Methodology → [LESS] Equipment details**.
        ") 