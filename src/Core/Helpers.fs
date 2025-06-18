namespace ARCSummary


open ARCtrl
open ARCtrl.Helper
open System
open System.Text
open System.Text.RegularExpressions
open SummaryTypes

module Option = 

    let toString (o : option<'a>) : string =
        match o with
        | Some value -> value.ToString()
        | None -> "" 
                               
    let verifyList (list: list<'a>) = // not yet implemented or partially or List.choose id
        if list.IsEmpty then failwith "Consider adding information to this list"
        else list
    
module StringHelper =
    let join (sep : string) (vals : string array) =
        String.Join(sep, vals)


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

// functions yet to be included in ARCtrl
module TableHelpers = 
    let dataIsEmpty (data:Data) =
        match data with
        | d -> d.ID.IsNone && d.Name.IsNone && d.DataType.IsNone && d.Format.IsNone && d.SelectorFormat.IsNone && d.Comments.Count = 0 

    let cellIsEmpty (cell:CompositeCell) = 
        match cell with
        | CompositeCell.Term oa  -> oa.isEmpty()
        | CompositeCell.FreeText s -> String.IsNullOrWhiteSpace s
        | CompositeCell.Unitized (v, oa) -> String.IsNullOrWhiteSpace v && oa.isEmpty()
        | CompositeCell.Data d -> dataIsEmpty d

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

open StringHelper
open Formating
open TableHelpers
open ArcTable

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

    let tableColumnsAreEqual (preceedingTable: ArcTable) (succeedingTable: ArcTable) : bool =
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
    let tablesAreLinked (preceedingTables : ArcTables) (succeedingTables: ArcTables) : bool = 
        preceedingTables
        |> Seq.exists (fun preceedingTable -> 
            succeedingTables
            |> Seq.exists (fun succeedingTable ->
                tableColumnsAreEqual preceedingTable succeedingTable
            )
        )

    let studyOutputIsAssayInput (preceedingStudy : ArcStudy) (succeedingAssay : ArcAssay) : bool = // previous linkAssayToStudy
        preceedingStudy.Tables
        |> Seq.exists (fun preceedingTable -> 
            succeedingAssay.Tables
            |> Seq.exists (fun succeedingTable ->
                tableColumnsAreEqual preceedingTable succeedingTable
            )
        )

    let associatedStudiesForAssay (investigation:ArcInvestigation) (assay:ArcAssay) =
        investigation.Studies
        |> Seq.choose (fun (study:ArcStudy) -> 
            if studyOutputIsAssayInput study assay then 
                Some study.Identifier
            else None 
        )
        // Seq.choose id necessary?
        |> Seq.distinct
        |> Seq.toList


    let associatedAssaysForStudy (investigation:ArcInvestigation) (study:ArcStudy) =
        investigation.Assays
        |> Seq.choose (fun (assay:ArcAssay) ->
            if studyOutputIsAssayInput study assay then 
                Some assay.Identifier 
            else None
        )
        // Seq.choose id necessary?
        |> Seq.distinct
        |> Seq.toList


    // Repetitive code, adapt into more generic function


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

    // only one way really necessary
    // Helper function for String.join with "," as seperators, consider adding and for the last word and capping the first word in a map

    let getOntologyHeaders (oaList:OntologyAnnotation list) =
        oaList
        |> List.choose (fun (oa:OntologyAnnotation) -> 
            if not (String.IsNullOrWhiteSpace oa.NameText) then 
                Some oa.NameText
            else None
        )

    let getAllOntologyInfos (headers: string list) (table:ArcTable) =  
        if not headers.IsEmpty then
            headers
            |> List.choose(fun (header:string) -> 
                let cellStrings : string array =            
                    getCellsByHeaderOntology table header
                    |> Array.distinct
                    |> Array.choose(fun (cell:CompositeCell) -> 
                        if not (cellIsEmpty cell) then
                            let oaID = removeHashAndNumbers header 
                            Some (String.Join(": ", oaID,cell.ToString()))
                        else None
                    )
                    |> Array.distinct // in case only one check for ontology is applied
                let grouped =
                    cellStrings
                    |> Array.groupBy(fun s -> s.Split(':').[0].Trim())
                    |> Array.choose(fun (key:string, values:string array) ->
                        let vals = 
                            values 
                            |> Array.map (fun s ->
                                let parts = s.Split(':')
                                if parts.Length > 1 then 
                                    parts.[1].Trim() 
                                else "" )
                            |> Array.filter (fun s -> not (String.IsNullOrWhiteSpace s))
                        if not (Array.isEmpty vals) then
                            let joined = String.concat ", " vals
                            Some (String.Join(": ", key, joined))
                        else None 
                )
                let resultingHeader = join "," grouped
                if not (String.IsNullOrWhiteSpace resultingHeader) then 
                    Some resultingHeader
                else None
            )
            |> List.toArray
            |> Array.filter (fun s -> not (String.IsNullOrWhiteSpace s))
            |> join " ; "
        else ""

    let getPreviousTablesForTable (currentTable: ArcTable) (currentID:string) (allTableIDs:(string*ArcTable) list) =
        allTableIDs 
        |> List.choose(fun (precedingID:string, precedingTable:ArcTable) -> 
            if precedingID <> currentID then 
                if tableColumnsAreEqual precedingTable currentTable then 
                    Some $"{precedingID} --> {currentID}"
                else None 
            else None
        )

    let collectAllTableIDs (investigation: ArcInvestigation) = 
        let studyTables = 
            investigation.Studies
            |> Seq.collect(fun (study:ArcStudy) ->
                study.Tables 
                |> Seq.map(fun (table:ArcTable) -> $"Study_{study.Identifier}_{table.Name}",table)
            )
        let assayTables =
            investigation.Assays
            |> Seq.collect(fun (assay:ArcAssay) ->
                assay.Tables 
                |> Seq.map(fun (table:ArcTable) -> $"Assay_{assay.Identifier}_{table.Name}",table)
            )
        Seq.append studyTables assayTables
        |> List.ofSeq

    let getAllTableNodes (investigation: ArcInvestigation) =
        let allTableIDs =
            collectAllTableIDs investigation
        allTableIDs
        |> List.collect(fun (id:string,table:ArcTable) -> getPreviousTablesForTable table id allTableIDs)

    let allStudyIDNodes (nodes:string list) =
        nodes 
        |> List.filter(fun (id:string) -> id.StartsWith("Study"))

    let allAssayIDNodes (nodes:string list) =
        nodes 
        |> List.filter(fun (id:string) -> id.StartsWith("Assay"))



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