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

module ArcTable =    //

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

module ArcQuerying = 

    // predicate replaced by selectColumn
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

    let getTimepoints (table:ArcTable) =
        let selectColumn =
            ArcTable.tryGetColumnByHeaderBy (fun (header:CompositeHeader) -> 
            match header with
            | CompositeHeader.Parameter oa -> oa.NameText = "Time point"
            //   | CompositeHeader.Factor oa -> oa.NameText = "Time point"
            | _ -> false
            ) table
        match selectColumn with 
                | Some col ->  
                        col.Cells
                        |> Array.map (fun (cell:CompositeCell) -> cell.AsTerm.NameText)
                        |> Array.distinct
                        |> Array.length
                | None -> 0

    // Anmerkung: Was ist bei samples die mehr als einmal vorkommen? Reicht hier Seq.max?
    let getSampleCount (arcTables:ResizeArray<ArcTable>) =
        arcTables
        |> ResizeArray.map (fun (table:ArcTable) -> table.RowCount)
        |> Seq.max

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

    let nodesEqual (node1:CompositeCell) (node2:CompositeCell) : bool =
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
                    nodesEqual inNode outNode
                )
            )
        | _ -> false

    // Anmerkung: Naming der Funktion. Rückgabewert ist bool, also eher "is", "are" oder ähnliches verwenden
    let linkTables (preceedingTables : ArcTables) (succeedingTables: ArcTables) : bool =
        preceedingTables
        |> Seq.exists (fun preceedingTable -> 
            succeedingTables
            |> Seq.exists (fun succeedingTable ->
                linkTablesByProcessNodes preceedingTable succeedingTable
            )
        )

    // Anmerkung: Naming der Funktion. Rückgabewert ist bool, also eher "is", "are" oder ähnliches verwenden
    let linkAssayToStudy (preceedingStudy : ArcStudy) (succeedingAssay : ArcAssay) : bool =
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

    // Anmerkung: "For" (CamelCase)
    // Anmerkung: Assay(s)?
    let getPreviousStudyIdentifiersforAssays (assay: ArcAssay) (investigation: ArcInvestigation) : list<string> =
        investigation.Studies
        |> Seq.choose (fun (study: ArcStudy) ->
            if linkTables study assay then
                Some study.Identifier 
            else None
            ) 
        |> Seq.toList 

    // Anmerkung: "For" (CamelCase)
    // Anmerkung: Assay(s)?
    let getPreviousAssayIdentifiersforAssays (assay: ArcAssay) (investigation: ArcInvestigation) : list<string> =
        investigation.Assays
        |> Seq.choose (fun (precedingAssay: ArcAssay) ->
            if precedingAssay.Identifier <> assay.Identifier then 
                if linkTables precedingAssay assay then
                    Some precedingAssay.Identifier 
                else None
            else None
            )
        |> Seq.toList 

    // Anmerkung: "For" (CamelCase)
    // Anmerkung: Assay(s)?
    let getFollowingStudyIdentifiersforAssays (assay: ArcAssay) (investigation: ArcInvestigation) : list<string> =
        let assayTables = ArcTables.ofSeq(assay.Tables)
        investigation.Studies
        |> Seq.choose (fun (study: ArcStudy) ->
            let studyTables = ArcTables.ofSeq(study.Tables)
            if linkTables assayTables studyTables then
                Some study.Identifier 
            else None
            ) 
        |> Seq.toList 

    // Anmerkung: "For" (CamelCase)
    // Anmerkung: Assay(s)?
    let getFollowingAssayIdentifiersforAssays (assay: ArcAssay) (investigation: ArcInvestigation) : list<string> =   
        let assayTables = ArcTables.ofSeq(assay.Tables)        
        investigation.Assays
        |> Seq.choose (fun (followingAssay: ArcAssay) ->
            if followingAssay.Identifier <> assay.Identifier then 
                let followingAssayTables = ArcTables.ofSeq(followingAssay.Tables)
                if linkTables  assayTables followingAssayTables then
                    Some followingAssay.Identifier 
                else None
            else None
            )
        |> Seq.toList 


    // Anmerkung: "For" (CamelCase)
    // Anmerkung: Study(ies)?
    let getPreviousStudyIdentifiersforStudies (study: ArcStudy) (investigation: ArcInvestigation) : list<string> =
        investigation.Studies
        |> Seq.choose (fun (precedingStudy: ArcStudy) ->
            if precedingStudy.Identifier <> study.Identifier then 
                if linkTables precedingStudy study then
                    Some precedingStudy.Identifier 
                else None
            else None
            )
        |> Seq.toList 

    // Anmerkung: "For" (CamelCase)
    // Anmerkung: Study(ies)?
    let getPreviousAssayIdentifiersforStudies (study: ArcStudy) (investigation: ArcInvestigation) : list<string> =
        investigation.Assays
        |> Seq.choose (fun (assay: ArcAssay) ->
            if linkTables assay study then
                Some assay.Identifier 
            else None
            ) 
        |> Seq.toList 

    // Anmerkung: "For" (CamelCase)
    // Anmerkung: Study(ies)?
    let getFollowingStudyIdentifiersforStudies (study: ArcStudy) (investigation: ArcInvestigation) : list<string> =
        let studyTables = ArcTables.ofSeq(study.Tables)
        investigation.Studies    
        |> Seq.choose (fun (followingStudy: ArcStudy) ->
            if followingStudy.Identifier <> study.Identifier then 
                let followingStudyTables = ArcTables.ofSeq(followingStudy.Tables)
                if linkTables  studyTables followingStudyTables then
                    Some followingStudy.Identifier 
                else None
            else None
            )
        |> Seq.toList 

    // Anmerkung: "For" (CamelCase)
    // Anmerkung: Study(ies)?
    let getFollowingAssayIdentifiersforStudies (study: ArcStudy) (investigation: ArcInvestigation) : list<string> =
        let studyTables = ArcTables.ofSeq(study.Tables)
        investigation.Assays
        |> Seq.choose (fun (assay: ArcAssay) ->
            let assayTables = ArcTables.ofSeq(assay.Tables)
            if linkTables studyTables assayTables  then
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

    let getHyperlinks (anheaders:list<OntologyAnnotation>) = 
        let formated =
            anheaders 
            |> List.map (fun (oa:OntologyAnnotation) ->  
                let name = oa.NameText |> removeHashAndNumbers |> removeUnderscoreAndNumbers
                let link = oa.TermAccessionOntobeeUrl
                let linklength = oa.TermAccessionOntobeeUrl |> Seq.length
                if linklength > 1 then 
                    $"[{name}]({link})"
                else $"`{name}`")
        String.Join(",",formated)


module TemplateHelpers = // Better names


    // mermaid graph Helpers

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

    let relationshipGraph (studyOVs:seq<StudyOverview>) (assayOVs:seq<AssayOverview>) =
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

    let getMermaidGraph (investigation:ArcInvestigation) (assayOVs:seq<AssayOverview>) (studyOVs:seq<StudyOverview>) : string =
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
        sb.AppendLine($"{relationshipGraph studyOVs assayOVs}") |> ignore
        sb.AppendLine($"class {studyClass} study;") |> ignore
        sb.AppendLine($"class {assayClass} assay;") |> ignore
        sb.AppendLine("classDef assay fill:#4FB3D9,stroke:#333,stroke-width:2px,color:#3A3A3A;") |> ignore
        sb.AppendLine("classDef study fill:#B4CE82,stroke:#333,stroke-width:2px,color:#3A3A3A;") |> ignore
        sb.AppendLine("```") |> ignore
        
        sb.ToString()


    // create TOC with numbered list and bullet point for the options such as multiple assays
    //Reworked, but could be still improved (Check iff it works as intended)
    let getTableOfContents (tlm:TopLevelMetadata) (assayOVs : seq<AssayOverview>) (studyOVs:seq<StudyOverview>) =
        let sb = StringBuilder()
        sb.AppendLine($"## Table of Contents \n") |> ignore

        let createAnchor (prefix:string) (ids:seq<string>) =
            ids
            |> Seq.map (fun (id:string) ->
                $"     - [{id}](#{prefix}--{id.ToLower()})")
            |> String.concat "\n"

        let anchorS = createAnchor "study" (studyOVs |> Seq.map (fun (sOV:StudyOverview) -> sOV.StudyIdentifier))
        let anchorA = createAnchor "assay" (assayOVs |> Seq.map (fun (aOV:AssayOverview) -> aOV.AssayIdentifier))           

        if not tlm.Contacts.IsEmpty && not tlm.Publications.IsEmpty then 
            sb.AppendLine("1. [Contacts](#contacts)") |> ignore
            sb.AppendLine("\n 2. [Publication](#publication)") |> ignore
            sb.AppendLine("\n 3. Studies \n") |> ignore
            sb.AppendJoin("\n", anchorS) |> ignore
            sb.AppendLine("\n 4. Assays \n") |> ignore     
            sb.AppendJoin("\n", anchorA) |> ignore    
            sb.AppendLine("\n 5. [References](#references)") |> ignore    

        else 
            sb.AppendLine("\n 1. Studies \n") |> ignore
            sb.AppendJoin("\n", anchorS) |> ignore
            sb.AppendLine("\n 2. Assays \n") |> ignore
            sb.AppendJoin("\n", anchorA) |> ignore
            sb.AppendLine("\n 3. [References](#references)") |> ignore
 
        sb.ToString()

// Instead of StringBuilder appproach it could be manipulated within a ResizeArray<string> like tocSection.Add and later String.concat "\n\n" to seperate within text

