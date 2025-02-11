namespace ARCSummary


open ARCtrl
open ARCtrl.Helper
open System
open System.Text
open System.Text.RegularExpressions

module Option = 

    let toString (o : option<'a>) : string =
        match o with
        | Some value -> value.ToString()
        | None -> "" 
                               
    let verifyList (list: list<'a>) = // not yet implemented or partially
        if list.IsEmpty then failwith "Consider adding information to this list"
        else list

module ArcTable =    // Functions to access information from ArcTables


    let getAllCharacteristics (table:ArcTable) =
        table.Headers
        |> Seq.choose (fun x ->
            match x with 
            | CompositeHeader.Characteristic c -> Some c
            | _ -> None )
        |> Seq.toList

    let getAllParameters (table:ArcTable) =
        table.Headers
        |> Seq.choose (fun x ->
            match x with 
            | CompositeHeader.Parameter p -> Some p
            | _ -> None )
        |> Seq.toList

    let getAllFactors (table:ArcTable) =
        table.Headers
        |> Seq.choose (fun x ->
            match x with 
            | CompositeHeader.Factor f -> Some f
            | _ -> None )
        |> Seq.toList

    let tryTerm (header : CompositeHeader) = 
            match header with
            | CompositeHeader.Characteristic oa
            | CompositeHeader.Component oa
            | CompositeHeader.Parameter oa
            | CompositeHeader.Factor oa -> Some oa
            | _ -> None

    let getCellsByHeaderOntology (table : ArcTable) (ontologyName : string) = 
            let isOntologyHeader (header : CompositeHeader)= 
                    match tryTerm header with
                    | Some oa -> oa.NameText = ontologyName 
                    | None -> false     
            let colOption = ArcTable.tryGetColumnByHeaderBy isOntologyHeader table
            match colOption with 
            | Some col ->  
                    col.Cells
            | None -> [||]

module ArcQuerying = // Functions for direct querying such as specific ontology search

    let getReplicates (table:ArcTable) =
        let selectColumn =
            ArcTable.tryGetColumnByHeaderBy (fun (header:CompositeHeader) -> 
            match header with
            | CompositeHeader.Characteristic oa -> oa.NameText = "Biological replicate"
            | _ -> false
            ) table
        match selectColumn with 
                | Some col ->  
                        col.Cells
                        |> Array.map (fun (cell:CompositeCell) -> cell.AsTerm.NameText)
                        |> Array.distinct
                        |> Seq.map (fun (v:string) -> int v)
                        |> Seq.max
                | None -> 0

    let getTimepoints (table:ArcTable) = // This function currently accounts for Timepoints that are defined as a Parameter, but some ARCs use Timepoints as a Factor instead
        let selectColumn =
            ArcTable.tryGetColumnByHeaderBy (fun (header:CompositeHeader) -> 
            match header with
            | CompositeHeader.Parameter oa -> oa.NameText = "Time point"
            | _ -> false
            ) table
        match selectColumn with 
                | Some col ->  
                        col.Cells
                        |> Array.map (fun (cell:CompositeCell) -> cell.AsTerm.NameText)
                        |> Array.distinct
                        |> Array.length
                | None -> 0

    let getSampleCount (arcTables:ResizeArray<ArcTable>) =
        arcTables
        |> ResizeArray.map (fun (table:ArcTable) -> table.RowCount)
        |> Seq.max // if samples occur repeatedly there might be a need for Seq.distinct here

    let getDataFiles (table:ArcTable) =
        let selectColumn =
            ArcTable.tryGetColumnByHeaderBy (fun (header:CompositeHeader) -> 
            match header with
            | CompositeHeader.Input IOType.Data
            | CompositeHeader.Output IOType.Data -> true
            | _ -> false
            ) table
        match selectColumn with 
                | Some col ->  
                        col.Cells
                        |> Array.length
                | None -> 0

    let getOntologyListByHeaderOntology (table : ArcTable) (ontologyName : string) = 
            let isOntologyHeader (header : CompositeHeader)= 
                    match ArcTable.tryTerm header with
                    | Some oa -> oa.NameText = ontologyName 
                    | None -> false     
            let colOption = ArcTable.tryGetColumnByHeaderBy isOntologyHeader table
            match colOption with 
            | Some col ->  
                    col.Cells
                    |> Array.map (fun (cell:CompositeCell) -> cell.AsTerm)
                    |> Array.distinct
                    |> List.ofArray
            | None -> []

    let getOrganisms (table:ArcTable) =
        getOntologyListByHeaderOntology table "Organism"

    let getMeasurementDevice (table:ArcTable) =
        getOntologyListByHeaderOntology table "Measurement Device"

    let getGenotype (table:ArcTable) =
        getOntologyListByHeaderOntology table "Genotype"

    let nodesAreEqual (node1:CompositeCell) (node2:CompositeCell) : bool =
        node1.ToString() = node2.ToString()    

    // Anmerkung: Naming der Funktion. Rückgabewert ist bool, also eher "is", "are" oder ähnliches verwenden
    let linkTablesByProcessNodes (preceedingTable: ArcTable) (succeedingTable: ArcTable) : bool =
        let outCol = preceedingTable.TryGetOutputColumn()
        let inCol = succeedingTable.TryGetInputColumn()
        match inCol,outCol with 
        | Some inCol, Some outCol -> 
            inCol.Cells
            |> Seq.exists (fun inNode ->
                outCol.Cells
                |> Seq.exists (fun outNode ->
                    nodesAreEqual inNode outNode
                )
            )
        | _ -> false

    // Anmerkung: Naming der Funktion. Rückgabewert ist bool, also eher "is", "are" oder ähnliches verwenden
    let tablesAreLinked (preceedingTables : ArcTables) (succeedingTables: ArcTables) : bool = //previous linkTables
        preceedingTables
        |> Seq.exists (fun preceedingTable -> 
            succeedingTables
            |> Seq.exists (fun succeedingTable ->
                linkTablesByProcessNodes preceedingTable succeedingTable
            )
        )

    // Anmerkung: Naming der Funktion. Rückgabewert ist bool, also eher "is", "are" oder ähnliches verwenden
    let linkAssayToStudy (preceedingStudy : ArcStudy) (succeedingAssay : ArcAssay) : bool = // previous linkAssayToStudy
        preceedingStudy.Tables
        |> Seq.exists (fun preceedingTable -> 
            succeedingAssay.Tables
            |> Seq.exists (fun succeedingTable ->
                linkTablesByProcessNodes preceedingTable succeedingTable
            )
        )

    // Anmerkung: Naming der Funktion. Rückgabewert ist bool, also eher "is", "are" oder ähnliches verwenden
    // Anmerkung: Assay fehlt im Namen, oder alternativ über Module
    // Anmerkung: filter und map können zu choose zusammengefasst werden
    let associatedStudies (investigation:ArcInvestigation) (assay:ArcAssay) =
        investigation.Studies
        |> ResizeArray.filter (fun (study:ArcStudy) -> linkAssayToStudy study assay)
        |> ResizeArray.map (fun (study:ArcStudy) -> study.Identifier)
        |> Seq.distinct
        |> Seq.toList

    // Anmerkung: Naming der Funktion. Rückgabewert ist bool, also eher "is", "are" oder ähnliches verwenden
    // Anmerkung: Study fehlt im Namen, oder alternativ über Module
    // Anmerkung: filter und map können zu choose zusammengefasst werden
    let associatedAssays (investigation:ArcInvestigation) (study:ArcStudy) =
        investigation.Assays
        |> ResizeArray.filter (fun (assay:ArcAssay) -> linkAssayToStudy study assay)
        |> ResizeArray.map (fun (assay:ArcAssay) -> assay.Identifier)
        |> Seq.distinct
        |> Seq.toList


    // Anmerkung: Sehr viel repetitiver Code ab hier. Versuch mal den generischen Teil in eine Funktion auszulagern   



    let getPreviousStudyIdsForAssay (assay: ArcAssay) (investigation: ArcInvestigation) : list<string> =
        investigation.Studies
        |> Seq.choose (fun (study: ArcStudy) ->
            if tablesAreLinked study assay = true then
                Some study.Identifier 
            else None
            ) 
        |> Seq.toList 


    let getPreviousAssayIdsForAssay (assay: ArcAssay) (investigation: ArcInvestigation) : list<string> =
        investigation.Assays
        |> Seq.choose (fun (precedingAssay: ArcAssay) ->
            if precedingAssay.Identifier <> assay.Identifier then 
                if tablesAreLinked precedingAssay assay = true then
                    Some precedingAssay.Identifier 
                else None
            else None
            )
        |> Seq.toList 

    let getFollowingStudyIdsForAssay (assay: ArcAssay) (investigation: ArcInvestigation) : list<string> =
        let assayTables = ArcTables.ofSeq(assay.Tables)
        investigation.Studies
        |> Seq.choose (fun (study: ArcStudy) ->
            let studyTables = ArcTables.ofSeq(study.Tables)
            if tablesAreLinked assayTables studyTables = true then
                Some study.Identifier 
            else None
            ) 
        |> Seq.toList 

    let getFollowingAssayIdsForAssay (assay: ArcAssay) (investigation: ArcInvestigation) : list<string> =   
        let assayTables = ArcTables.ofSeq(assay.Tables)        
        investigation.Assays
        |> Seq.choose (fun (followingAssay: ArcAssay) ->
            if followingAssay.Identifier <> assay.Identifier then 
                let followingAssayTables = ArcTables.ofSeq(followingAssay.Tables)
                if tablesAreLinked  assayTables followingAssayTables = true then
                    Some followingAssay.Identifier 
                else None
            else None
            )
        |> Seq.toList 


    let getPreviousStudyIdsForStudy (study: ArcStudy) (investigation: ArcInvestigation) : list<string> =
        investigation.Studies
        |> Seq.choose (fun (precedingStudy: ArcStudy) ->
            if precedingStudy.Identifier <> study.Identifier then 
                if tablesAreLinked precedingStudy study = true then
                    Some precedingStudy.Identifier 
                else None
            else None
            )
        |> Seq.toList 

    let getPreviousAssayIdsForStudy (study: ArcStudy) (investigation: ArcInvestigation) : list<string> =
        investigation.Assays
        |> Seq.choose (fun (assay: ArcAssay) ->
            if tablesAreLinked assay study = true then
                Some assay.Identifier 
            else None
            ) 
        |> Seq.toList 

    let getFollowingStudyIdsForStudy (study: ArcStudy) (investigation: ArcInvestigation) : list<string> =
        let studyTables = ArcTables.ofSeq(study.Tables)
        investigation.Studies    
        |> Seq.choose (fun (followingStudy: ArcStudy) ->
            if followingStudy.Identifier <> study.Identifier then 
                let followingStudyTables = ArcTables.ofSeq(followingStudy.Tables)
                if tablesAreLinked  studyTables followingStudyTables = true then
                    Some followingStudy.Identifier 
                else None
            else None
            )
        |> Seq.toList 

    let getFollowingAssayIdsForStudy (study: ArcStudy) (investigation: ArcInvestigation) : list<string> =
        let studyTables = ArcTables.ofSeq(study.Tables)
        investigation.Assays
        |> Seq.choose (fun (assay: ArcAssay) ->
            let assayTables = ArcTables.ofSeq(assay.Tables)
            if tablesAreLinked studyTables assayTables = true  then
                Some assay.Identifier 
            else None
            ) 
        |> Seq.toList 


    // Helper function for String.join with "," as seperators, consider adding and for the last word and capping the first word in a map
module Formating =
    let removeHashAndNumbers (input: string) =
        Regex.Replace(input, @"#\d+", "")
    let removeUnderscoreAndNumbers (input:string) =
        Regex.Replace(input, @"_\d+", "")

    let getHyperlinks (annotationheaders:list<OntologyAnnotation>) = 
        let formated =
            annotationheaders 
            |> List.map (fun (oa:OntologyAnnotation) ->  
                let name = oa.NameText |> removeHashAndNumbers |> removeUnderscoreAndNumbers
                let link = oa.TermAccessionOntobeeUrl
                let linklength = oa.TermAccessionOntobeeUrl |> Seq.length
                if linklength > 1 then 
                    $"[{name}]({link})"
                else $"`{name}`")
        String.Join(",",formated)


module TemplateHelpers = // Better names


    //Changes mermaid graph Helpers

    let studyToAssayRelationships (studyOVs:seq<StudyOverview>) = 
        studyOVs
        |> Seq.collect (fun (sOV: StudyOverview) ->
            let studyId = $"{sOV.StudyIdentifier}_study"
            sOV.FollowingAssayIdentifiers 
            |> Seq.map (fun assayId -> $"{studyId} --> {assayId}_assay")
        )
        |> Seq.toArray

    let studyToStudyRelationships (studyOVs:seq<StudyOverview>) = 
        studyOVs
        |> Seq.collect (fun (sOV: StudyOverview) ->
            let studyId = $"{sOV.StudyIdentifier}_study"
            sOV.FollowingStudyIdentifiers
            |> Seq.map (fun studyID -> $"{studyId} --> {studyID}_study")
        )
        |> Seq.toArray

    let assayToAssayRelationships (assayOVs:seq<AssayOverview>) = 
        assayOVs
        |> Seq.collect (fun (aOV: AssayOverview) ->
            let assayId = $"{aOV.AssayIdentifier}_assay"
            aOV.FollowingAssayIdentifiers 
            |> Seq.map (fun assayID -> $"{assayId} --> {assayID}_assay")
        )
        |> Seq.toArray

    let assayToStudyRelationships (assayOVs:seq<AssayOverview>) = 
        assayOVs
        |> Seq.collect (fun (aOV: AssayOverview) ->
            let assayId = $"{aOV.AssayIdentifier}_assay"
            aOV.FollowingStudyIdentifiers 
            |> Seq.map (fun studyId -> $"{assayId} --> {studyId}_study")
        )
        |> Seq.toArray

    let mermaidGraphNodes (studyOVs:seq<StudyOverview>) (assayOVs:seq<AssayOverview>) =
        let comb =
            [|
                studyToAssayRelationships studyOVs
                studyToStudyRelationships studyOVs
                assayToAssayRelationships assayOVs
                assayToStudyRelationships assayOVs
            |]
            |> Array.concat
        if comb.Length = 0 then ""
        else 
            comb 
            |> Array.reduce (fun acc current -> $"{acc};\n{current} ")    

    let getRelationshipGraph (investigation:ArcInvestigation) (assayOVs:seq<AssayOverview>) (studyOVs:seq<StudyOverview>) : string =
        let sb = StringBuilder()

        let studyNodes = 
            studyOVs 
            |> Seq.map (fun (sOV: StudyOverview) -> $"{sOV.StudyIdentifier}_study[\"{sOV.StudyIdentifier}\"];")
            |> String.concat "\n"

        let assayNodes = 
            assayOVs 
            |> Seq.map (fun (aOV: AssayOverview) -> $"{aOV.AssayIdentifier}_assay[\"{aOV.AssayIdentifier}\"];")
            |> String.concat "\n"

        let studyClass =
            studyOVs
            |> Seq.map (fun (sOV: StudyOverview) -> $"{sOV.StudyIdentifier}_study")
            |> String.concat ","

        let assayClass =
            assayOVs
            |> Seq.map (fun (aOV: AssayOverview) -> $"{aOV.AssayIdentifier}_assay")
            |> String.concat ","
        // this here is necessary:
        sb.AppendLine($"
```mermaid
---
title: {investigation.Identifier}
---
graph TB") |> ignore
        sb.AppendLine(studyNodes) |> ignore
        sb.AppendLine(assayNodes) |> ignore
        sb.AppendLine($"{mermaidGraphNodes studyOVs assayOVs}") |> ignore
        sb.AppendLine($"class {studyClass} study;") |> ignore
        sb.AppendLine($"class {assayClass} assay;") |> ignore
        sb.AppendLine("classDef assay fill:#4FB3D9,stroke:#333,stroke-width:2px,color:#3A3A3A;") |> ignore
        sb.AppendLine("classDef study fill:#B4CE82,stroke:#333,stroke-width:2px,color:#3A3A3A;") |> ignore
        sb.AppendLine("```") |> ignore
        
        sb.ToString()


// Function to add the legend either as subgraph or below as seperate graph

// ---
// title: Legend 
// ---
// graph TB
//     study_legend["Study"]:::study
//     assay_legend["Assay"]:::assay
// classDef assay fill:#4FB3D9,stroke:#3A3A3A,stroke-width:2px,color:#3A3A3A;
// classDef study fill:#B4CE82,stroke:#3A3A3A,stroke-width:2px,color:#3A3A3A;
// subgraph directions LR