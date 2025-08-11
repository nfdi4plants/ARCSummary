//namespace ARCSummary


#r "nuget: ARCtrl, 3.0.0-alpha.4"


open ARCtrl
open System
open System.Text
open System.Text.RegularExpressions

module StringHelper =
    let join (sep : string) (vals : string array) =
        String.Join(sep, vals)
open StringHelper

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

open Formating
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
                            if vals.Length < 20 then
                                let joined = String.concat ", " vals
                                Some (String.Join(": ", key, joined))
                            else 
                                let joined = 
                                    let head = vals |> List.ofArray |> List.head
                                    let last = vals |> List.ofArray |> List.last
                                    let count = vals.Length - 2
                                    $"{head} ... {last} +{count} more"
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
        |> List.toArray
        |> join ","

    let allAssayIDNodes (nodes:string list) =
        nodes 
        |> List.filter(fun (id:string) -> id.StartsWith("Assay"))
        |> List.toArray
        |> join ","


open TableHelpers

let sampleARC = ARC.load("/Users/olescholz/Desktop/Results/Sample_ARCs/Historical_Phenomics_Wheat_Collection_IPK")

let sampleARC2 = ARC.load("/Users/olescholz/Desktop/Results/Sample_ARCs/Ru_ChlamyHeatstress")

// let table =     
//     sampleARC.Studies.[0].Tables.[0]

// let res =
//     getAllOntologyInfos (getOntologyHeaders(getAllCharacteristics table)) table

// printfn "%s" res

let provenance (investigation:ArcInvestigation) = 
    let sb= StringBuilder()
    sb.AppendLine($"### Study Table Associations (indicate with --> an input/output link): \n {allStudyIDNodes (getAllTableNodes investigation)} \n") |> ignore
    sb.AppendLine($"### Assay Table Associations (indicate with --> an input/output link): \n {allAssayIDNodes (getAllTableNodes investigation)} \n")

let res = 
    provenance sampleARC

res.ToString()


let hfisdi = 1 + 1
// module ArcTable =    // Functions to access information from ArcTables


//     let getAllCharacteristics (table:ArcTable) =
//         table.Headers
//         |> Seq.choose (fun x ->
//             match x with 
//             | CompositeHeader.Characteristic c -> Some c
//             | _ -> None )
//         |> Seq.toList

//     let getAllParameters (table:ArcTable) =
//         table.Headers
//         |> Seq.choose (fun x ->
//             match x with 
//             | CompositeHeader.Parameter p -> Some p
//             | _ -> None )
//         |> Seq.toList

//     let getAllFactors (table:ArcTable) =
//         table.Headers
//         |> Seq.choose (fun x ->
//             match x with 
//             | CompositeHeader.Factor f -> Some f
//             | _ -> None )
//         |> Seq.toList

//     let tryTerm (header : CompositeHeader) = 
//             match header with
//             | CompositeHeader.Characteristic oa
//             | CompositeHeader.Component oa
//             | CompositeHeader.Parameter oa
//             | CompositeHeader.Factor oa -> Some oa
//             | _ -> None

//     let getCellsByHeaderOntology (table : ArcTable) (ontologyName : string) = 
//             let isOntologyHeader (header : CompositeHeader)= 
//                     match tryTerm header with
//                     | Some oa -> oa.NameText = ontologyName 
//                     | None -> false     
//             let colOption = ArcTable.tryGetColumnByHeaderBy isOntologyHeader table
//             match colOption with 
//             | Some col ->  
//                     col.Cells
//             | None -> [||]

// open StringHelper
// open Formating
// open TableHelpers
// open ArcTable

// module ArcQuerying = // Functions for direct querying such as specific ontology search

//     let getReplicates (table:ArcTable) =
//         let selectColumn =
//             ArcTable.tryGetColumnByHeaderBy (fun (header:CompositeHeader) -> 
//             match header with
//             | CompositeHeader.Characteristic oa -> oa.NameText = "Biological replicate"
//             | _ -> false
//             ) table
//         match selectColumn with 
//                 | Some col ->  
//                         col.Cells
//                         |> Array.map (fun (cell:CompositeCell) -> cell.AsTerm.NameText)
//                         |> Array.distinct
//                         |> Seq.map (fun (v:string) -> int v)
//                         |> Seq.max
//                 | None -> 0

//     let getTimepoints (table:ArcTable) = 
//         let selectColumn =
//             ArcTable.tryGetColumnByHeaderBy (fun (header:CompositeHeader) -> 
//             match header with
//             | CompositeHeader.Parameter oa 
//             | CompositeHeader.Factor oa -> oa.NameText = "Time point"
//             | _ -> false
//             ) table
//         match selectColumn with 
//                 | Some col -> 
//                     col.Cells 
//                     |> Array.choose(fun (cell:CompositeCell) ->
//                         match cell with 
//                         | cell when cell.isTerm -> Some cell.AsTerm.NameText
//                         | cell when cell.isUnitized -> 
//                             Some (cell.AsUnitized.ToString()) 
//                         | _ -> None 
//                     )
//                     |> Array.distinct
//                     |> Array.length
//                 | None -> 0

//     let getSampleCount (arcTables:ResizeArray<ArcTable>) =
//         arcTables
//         |> Seq.map (fun (table:ArcTable) -> table.RowCount)
//         |> Seq.max // if samples occur repeatedly there might be a need for Seq.distinct here

//     let getDataFiles (table:ArcTable) =
//         let selectColumn =
//             ArcTable.tryGetColumnByHeaderBy (fun (header:CompositeHeader) -> 
//             match header with
//             | CompositeHeader.Input IOType.Data
//             | CompositeHeader.Output IOType.Data -> true
//             | _ -> false
//             ) table
//         match selectColumn with 
//                 | Some col ->  
//                         col.Cells
//                         |> Array.length
//                 | None -> 0

//     let getOntologyListByHeaderOntology (table : ArcTable) (ontologyName : string) = 
//             let isOntologyHeader (header : CompositeHeader)= 
//                     match ArcTable.tryTerm header with
//                     | Some oa -> oa.NameText = ontologyName 
//                     | None -> false     
//             let colOption = ArcTable.tryGetColumnByHeaderBy isOntologyHeader table
//             match colOption with 
//             | Some col ->  
//                     col.Cells
//                     |> Array.map (fun (cell:CompositeCell) -> cell.AsTerm)
//                     |> Array.distinct
//                     |> List.ofArray
//             | None -> []

//     let getOrganisms (table:ArcTable) =
//         getOntologyListByHeaderOntology table "Organism"

//     let getMeasurementDevice (table:ArcTable) =
//         getOntologyListByHeaderOntology table "Measurement Device"

//     let getGenotype (table:ArcTable) =
//         getOntologyListByHeaderOntology table "Genotype"

//     let nodesAreEqual (node1:CompositeCell) (node2:CompositeCell) : bool =
//         node1.ToString() = node2.ToString()    

//     let tableColumnsAreEqual (preceedingTable: ArcTable) (succeedingTable: ArcTable) : bool =
//         let outCol = preceedingTable.TryGetOutputColumn()
//         let inCol = succeedingTable.TryGetInputColumn()
//         match inCol,outCol with 
//         | Some inCol, Some outCol -> 
//             inCol.Cells
//             |> Seq.exists (fun inNode ->
//                 outCol.Cells
//                 |> Seq.exists (fun outNode ->
//                     nodesAreEqual inNode outNode
//                 )
//             )
//         | _ -> false
//     let tablesAreLinked (preceedingTables : ArcTables) (succeedingTables: ArcTables) : bool = 
//         preceedingTables
//         |> Seq.exists (fun preceedingTable -> 
//             succeedingTables
//             |> Seq.exists (fun succeedingTable ->
//                 tableColumnsAreEqual preceedingTable succeedingTable
//             )
//         )

//     let studyOutputIsAssayInput (preceedingStudy : ArcStudy) (succeedingAssay : ArcAssay) : bool = // previous linkAssayToStudy
//         preceedingStudy.Tables
//         |> Seq.exists (fun preceedingTable -> 
//             succeedingAssay.Tables
//             |> Seq.exists (fun succeedingTable ->
//                 tableColumnsAreEqual preceedingTable succeedingTable
//             )
//         )

//     let associatedStudiesForAssay (investigation:ArcInvestigation) (assay:ArcAssay) =
//         investigation.Studies
//         |> Seq.choose (fun (study:ArcStudy) -> 
//             if studyOutputIsAssayInput study assay then 
//                 Some study.Identifier
//             else None 
//         )
//         // Seq.choose id necessary?
//         |> Seq.distinct
//         |> Seq.toList


//     let associatedAssaysForStudy (investigation:ArcInvestigation) (study:ArcStudy) =
//         investigation.Assays
//         |> Seq.choose (fun (assay:ArcAssay) ->
//             if studyOutputIsAssayInput study assay then 
//                 Some assay.Identifier 
//             else None
//         )
//         // Seq.choose id necessary?
//         |> Seq.distinct
//         |> Seq.toList


//     // Repetitive code, adapt into more generic function


//     let getPreviousStudyIdsForAssay (assay: ArcAssay) (investigation: ArcInvestigation) : list<string> =
//         investigation.Studies
//         |> Seq.choose (fun (study: ArcStudy) ->
//             if tablesAreLinked study assay = true then
//                 Some study.Identifier 
//             else None
//             ) 
//         |> Seq.toList 


//     let getPreviousAssayIdsForAssay (assay: ArcAssay) (investigation: ArcInvestigation) : list<string> =
//         investigation.Assays
//         |> Seq.choose (fun (precedingAssay: ArcAssay) ->
//             if precedingAssay.Identifier <> assay.Identifier then 
//                 if tablesAreLinked precedingAssay assay = true then
//                     Some precedingAssay.Identifier 
//                 else None
//             else None
//             )
//         |> Seq.toList 

//     let getFollowingStudyIdsForAssay (assay: ArcAssay) (investigation: ArcInvestigation) : list<string> =
//         let assayTables = ArcTables.ofSeq(assay.Tables)
//         investigation.Studies
//         |> Seq.choose (fun (study: ArcStudy) ->
//             let studyTables = ArcTables.ofSeq(study.Tables)
//             if tablesAreLinked assayTables studyTables = true then
//                 Some study.Identifier 
//             else None
//             ) 
//         |> Seq.toList 

//     let getFollowingAssayIdsForAssay (assay: ArcAssay) (investigation: ArcInvestigation) : list<string> =   
//         let assayTables = ArcTables.ofSeq(assay.Tables)        
//         investigation.Assays
//         |> Seq.choose (fun (followingAssay: ArcAssay) ->
//             if followingAssay.Identifier <> assay.Identifier then 
//                 let followingAssayTables = ArcTables.ofSeq(followingAssay.Tables)
//                 if tablesAreLinked  assayTables followingAssayTables = true then
//                     Some followingAssay.Identifier 
//                 else None
//             else None
//             )
//         |> Seq.toList 


//     let getPreviousStudyIdsForStudy (study: ArcStudy) (investigation: ArcInvestigation) : list<string> =
//         investigation.Studies
//         |> Seq.choose (fun (precedingStudy: ArcStudy) ->
//             if precedingStudy.Identifier <> study.Identifier then 
//                 if tablesAreLinked precedingStudy study = true then
//                     Some precedingStudy.Identifier 
//                 else None
//             else None
//             )
//         |> Seq.toList 

//     let getPreviousAssayIdsForStudy (study: ArcStudy) (investigation: ArcInvestigation) : list<string> =
//         investigation.Assays
//         |> Seq.choose (fun (assay: ArcAssay) ->
//             if tablesAreLinked assay study = true then
//                 Some assay.Identifier 
//             else None
//             ) 
//         |> Seq.toList 

//     let getFollowingStudyIdsForStudy (study: ArcStudy) (investigation: ArcInvestigation) : list<string> =
//         let studyTables = ArcTables.ofSeq(study.Tables)
//         investigation.Studies    
//         |> Seq.choose (fun (followingStudy: ArcStudy) ->
//             if followingStudy.Identifier <> study.Identifier then 
//                 let followingStudyTables = ArcTables.ofSeq(followingStudy.Tables)
//                 if tablesAreLinked  studyTables followingStudyTables = true then
//                     Some followingStudy.Identifier 
//                 else None
//             else None
//             )
//         |> Seq.toList 

//     let getFollowingAssayIdsForStudy (study: ArcStudy) (investigation: ArcInvestigation) : list<string> =
//         let studyTables = ArcTables.ofSeq(study.Tables)
//         investigation.Assays
//         |> Seq.choose (fun (assay: ArcAssay) ->
//             let assayTables = ArcTables.ofSeq(assay.Tables)
//             if tablesAreLinked studyTables assayTables = true  then
//                 Some assay.Identifier 
//             else None
//             ) 
//         |> Seq.toList 

// open StringHelper
// open Formating
// open TableHelpers
// open ArcQuerying
// module Promptdesign2 =

//     let nodesAreEqual (node1:CompositeCell) (node2:CompositeCell) : bool =
//         node1.ToString() = node2.ToString()    

//     let tableColumnsAreEqual (preceedingTable: ArcTable) (succeedingTable: ArcTable) : bool =
//         let outCol = preceedingTable.TryGetOutputColumn()
//         let inCol = succeedingTable.TryGetInputColumn()
//         match inCol,outCol with 
//         | Some inCol, Some outCol -> 
//             inCol.Cells
//             |> Seq.exists (fun inNode ->
//                 outCol.Cells
//                 |> Seq.exists (fun outNode ->
//                     nodesAreEqual inNode outNode
//                 )
//             )
//         | _ -> false
//     let getPreviousTablesForTable (currentTable: ArcTable) (currentID:string) (allTableIDs:(string*ArcTable) list) =
//         allTableIDs 
//         |> List.choose(fun (precedingID:string, precedingTable:ArcTable) -> 
//             if precedingID <> currentID then 
//                 if tableColumnsAreEqual precedingTable currentTable then 
//                     Some $"{precedingID} --> {currentID}"
//                 else None 
//             else None
//         )
//     let collectAllTableIDs (investigation: ArcInvestigation) = 
//         let studyTables = 
//             investigation.Studies
//             |> Seq.collect(fun (study:ArcStudy) ->
//                 study.Tables 
//                 |> Seq.map(fun (table:ArcTable) -> $"Study_{study.Identifier}_{table.Name}",table)
//             )
//         let assayTables =
//             investigation.Assays
//             |> Seq.collect(fun (assay:ArcAssay) ->
//                 assay.Tables 
//                 |> Seq.map(fun (table:ArcTable) -> $"Assay_{assay.Identifier}_{table.Name}",table)
//             )
//         Seq.append studyTables assayTables
//         |> List.ofSeq

//     let getAllTableNodes (investigation: ArcInvestigation) =
//         let allTableIDs =
//             collectAllTableIDs investigation
//         allTableIDs
//         |> List.collect(fun (id:string,table:ArcTable) -> getPreviousTablesForTable table id allTableIDs)

//     let allStudyIDNodes (nodes:string list) =
//         nodes 
//         |> List.filter(fun (id:string) -> id.StartsWith("Study"))
//         |> List.toArray
//         |> join "\n"

//     let allAssayIDNodes (nodes:string list) =
//         nodes 
//         |> List.filter(fun (id:string) -> id.StartsWith("Assay"))
//         |> List.toArray
//         |> join "\n"


//     let getOntologyHeaders (oaList:OntologyAnnotation list) =
//         oaList
//         |> List.choose (fun (oa:OntologyAnnotation) -> 
//             if not (String.IsNullOrWhiteSpace oa.NameText) then 
//                 Some oa.NameText
//             else None
//         )
    
//     let getAllCharacteristics (table:ArcTable) =
//         table.Headers
//         |> Seq.choose (fun x ->
//             match x with 
//             | CompositeHeader.Characteristic c -> Some c
//             | _ -> None )
//         |> Seq.toList

//     let getAllParameters (table:ArcTable) =
//         table.Headers
//         |> Seq.choose (fun x ->
//             match x with 
//             | CompositeHeader.Parameter p -> Some p
//             | _ -> None )
//         |> Seq.toList

//     let getAllFactors (table:ArcTable) =
//         table.Headers
//         |> Seq.choose (fun x ->
//             match x with 
//             | CompositeHeader.Factor f -> Some f
//             | _ -> None )
//         |> Seq.toList

//     let tryTerm (header : CompositeHeader) = 
//             match header with
//             | CompositeHeader.Characteristic oa
//             | CompositeHeader.Component oa
//             | CompositeHeader.Parameter oa
//             | CompositeHeader.Factor oa -> Some oa
//             | _ -> None
//     let getCellsByHeaderOntology (table : ArcTable) (ontologyName : string) = 
//             let isOntologyHeader (header : CompositeHeader)= 
//                     match tryTerm header with
//                     | Some oa -> oa.NameText = ontologyName 
//                     | None -> false     
//             let colOption = ArcTable.tryGetColumnByHeaderBy isOntologyHeader table
//             match colOption with 
//             | Some col ->  
//                     col.Cells
//             | None -> [||]
//     let getAllOntologyInfos (headers: string list) (table:ArcTable) =  
//         if not headers.IsEmpty then
//             headers
//             |> List.choose(fun (header:string) -> 
//                 let cellStrings : string array =            
//                     getCellsByHeaderOntology table header
//                     |> Array.distinct
//                     |> Array.choose(fun (cell:CompositeCell) -> 
//                         if not (cellIsEmpty cell) then
//                             let oaID = removeHashAndNumbers header 
//                             Some (String.Join(": ", oaID,cell.ToString()))
//                         else None
//                     )
//                     |> Array.distinct // in case only one check for ontology is applied
//                 let grouped =
//                     cellStrings
//                     |> Array.groupBy(fun s -> s.Split(':').[0].Trim())
//                     |> Array.choose(fun (key:string, values:string array) ->
//                         let vals = 
//                             values 
//                             |> Array.map (fun s ->
//                                 let parts = s.Split(':')
//                                 if parts.Length > 1 then 
//                                     parts.[1].Trim() 
//                                 else "" )
//                             |> Array.filter (fun s -> not (String.IsNullOrWhiteSpace s))
//                         if not (Array.isEmpty vals) then
//                             let joined = String.concat ", " vals
//                             Some (String.Join(": ", key, joined))
//                         else None 
//                 )
//                 let resultingHeader = join "," grouped
//                 if not (String.IsNullOrWhiteSpace resultingHeader) then 
//                     Some resultingHeader
//                 else None
//             )
//             |> List.toArray
//             |> Array.filter (fun s -> not (String.IsNullOrWhiteSpace s))
//             |> join " ; "
//         else ""


//     type TopLevelMetadata = { 
//         Title : string option ;
//         Description : string option ;
//         SubmissionDate : string option ;
//         PublicReleaseDate : string option ;
//         Publications : Publication list ;
//         Contacts : Person list ;
//         AssayIdentifiers : string list ;
//         AssayCount : int option ;
//         StudyIdentifiers : string list ;
//         StudyCount : int option
//     }
//     let createOverviewTable (tlm:TopLevelMetadata) : string =
//         let sb = StringBuilder()
        
//         sb.AppendLine("### Overview Table") |> ignore
//         sb.AppendLine("| Meta Data | Description |") |> ignore
//         sb.AppendLine("| --------- | ----------- |") |> ignore
//         if tlm.SubmissionDate.IsSome then
//             sb.AppendLine($"| Submission Date | {tlm.SubmissionDate.Value} |") |> ignore
//         else sb.AppendLine("| Submission Date  | tba |") |> ignore
//         if tlm.PublicReleaseDate.IsSome then
//             sb.AppendLine($"| Public Release Date | {tlm.PublicReleaseDate.Value} |") |> ignore
//         else sb.AppendLine("| Public Release Date | tba |") |> ignore
//         let studyIDs = String.Join(" , ", tlm.StudyIdentifiers)
//         sb.AppendLine($"| Study identifiers | {studyIDs} |") |> ignore
//         sb.AppendLine($"| Study Count | {tlm.StudyCount.Value} |") |> ignore
//         let assayIDs = String.Join(" , ", tlm.AssayIdentifiers)
//         sb.AppendLine($"| Assay identifiers | {assayIDs} |") |> ignore
//         sb.AppendLine($"| Assay Count | {tlm.AssayCount.Value} |") |> ignore

//         sb.ToString()
//     let getTopLevelMetadata (selectISA:ArcInvestigation) : TopLevelMetadata = {
//         Title = selectISA.Title ;
//         Description = selectISA.Description ;
//         SubmissionDate = selectISA.SubmissionDate ;
//         PublicReleaseDate = selectISA.PublicReleaseDate ;
//         Publications = selectISA.Publications |> Seq.toList ;
//         Contacts = selectISA.Contacts |> Seq.toList ;
//         AssayIdentifiers = selectISA.AssayIdentifiers |> Seq.toList ;
//         AssayCount = Some selectISA.AssayCount ;
//         StudyIdentifiers = selectISA.StudyIdentifiers |> Seq.toList ;
//         StudyCount = Some selectISA.StudyCount
//     }

//     type StudyOverview = {
//         StudyIdentifier : string ; 
//         StudyTitle : string option ;
//         StudyDescription : string option ;
//         TableCount : int option ;
//         TableNames : string list ; 
//         Characteristics : OntologyAnnotation list ; 
//         Organisms : OntologyAnnotation list ;
//         Genotypes : OntologyAnnotation list ;
//         BiologicalReplicateCount : int ; 
//         TimepointCount : int option ; 
//         SampleCount : int option ;
//         Parameters : OntologyAnnotation list ;
//         Factors : OntologyAnnotation list ;
//         AssociatedAssays : string list ; 
//         PreviousAssayIdentifiers : string list ;
//         PreviousStudyIdentifiers : string list ;
//         FollowingAssayIdentifiers : string list ;
//         FollowingStudyIdentifiers : string list ;
//         DataFileCount : int option
//     } 

//     type AssayOverview = {
//         AssayIdentifier : string ;
//         AssayTitle : string option ;
//         AssayDescription : string option ;
//         MeasurementType : OntologyAnnotation option ; 
//         MeasurementDevice : OntologyAnnotation list  ;    
//         TechnologyType : OntologyAnnotation option ;
//         TableCount: int option ;
//         TableNames: string list ; 
//         Characteristics : OntologyAnnotation list ;   
//         SampleCount : int option ;
//         Parameters : OntologyAnnotation list ; 
//         Factors : OntologyAnnotation list ;
//         AssociatedStudies : string list  ; 
//         PreviousAssayIdentifiers : string list ;
//         PreviousStudyIdentifiers : string list ;
//         FollowingAssayIdentifiers : string list ;
//         FollowingStudyIdentifiers : string list ;
//         DataFileCount : int option
//     }

//     let createStudyAdditionalDetails (sOV:StudyOverview) : string =
//         let sb = StringBuilder()
//         sb.AppendLine("| Meta Data | Description |") |> ignore
//         sb.AppendLine("| --------- | ----------- |") |> ignore
//         sb.AppendLine($"| Table Count | {sOV.TableCount.Value} |") |> ignore
//         let tableNamesString = String.Join(" , ", sOV.TableNames)
//         sb.AppendLine($"| Table Names | {tableNamesString} |") |> ignore
//         sb.AppendLine($"| Sample Count | {sOV.SampleCount.Value} |") |> ignore
//         sb.AppendLine($"| Data File Count | {sOV.DataFileCount.Value} |") |> ignore
//         let laString = String.Join(" , ", sOV.AssociatedAssays)
//         sb.AppendLine($"| Associated assays | {laString} |") |> ignore
//         if not sOV.Organisms.IsEmpty then
//             let organism = String.Join(" , ", (sOV.Organisms |> List.map (fun oa -> oa.NameText)))
//             sb.AppendLine($"| Organisms | _{organism}_ |") |> ignore
//         if not sOV.Genotypes.IsEmpty then
//             let genotype = String.Join(" , ", (sOV.Genotypes |> List.map (fun oa -> oa.NameText)))
//             sb.AppendLine($"| Genotype | _{genotype}_ |") |> ignore
//         sb.AppendLine($"| Biological replicates | {sOV.BiologicalReplicateCount} |") |> ignore
//         if sOV.TimepointCount.Value > 0 = true then 
//             let tps = String.Join(" , ", sOV.TimepointCount.Value)
//             sb.AppendLine($"| Time points | {tps} |") |> ignore
//         sb.ToString()
//     let createAssayAdditionalDetails (aOV:AssayOverview) : string =
//         let sb = StringBuilder()
//         sb.AppendLine("| Meta Data | Description |") |> ignore
//         sb.AppendLine("| --------- | ----------- |") |> ignore
//         if aOV.MeasurementType.IsSome = true then // change to = true
//             sb.AppendLine($"| Measurement Type | {aOV.MeasurementType.Value.NameText} |") |> ignore 
//         if not aOV.MeasurementDevice.IsEmpty then 
//             sb.AppendLine($"| Measurement Device | {aOV.MeasurementDevice.Head.Name.Value} |") |> ignore 
//         if aOV.TechnologyType.IsSome = true then
//             sb.AppendLine($"| Technology Type | {aOV.TechnologyType.Value.NameText} |") |> ignore
//         sb.AppendLine($"| Table Count | {aOV.TableCount.Value} |") |> ignore
//         let tableNamesString = String.Join(" , ", aOV.TableNames)
//         sb.AppendLine($"| Table Names | {tableNamesString} |") |> ignore
//         sb.AppendLine($"| Sample Count | {aOV.SampleCount.Value} |") |> ignore
//         sb.AppendLine($"| Data File Count | {aOV.DataFileCount.Value} |") |> ignore
//         if not aOV.AssociatedStudies.IsEmpty then
//             let associatedStudies= join "," (aOV.AssociatedStudies |> List.toArray)
//             sb.AppendLine($"| Associated studies | {associatedStudies} |") |> ignore
//         sb.ToString()
    
//     let getStudyOverview (investigation:ArcInvestigation) (study:ArcStudy) = {
//         StudyIdentifier = study.Identifier
//         StudyTitle = study.Title
//         StudyDescription = study.Description
//         TableCount = Some study.TableCount
//         TableNames = study.TableNames
//         Characteristics = study.Tables |> Seq.map ArcTable.getAllCharacteristics |> Seq.concat |> Seq.distinct |> Seq.toList
//         Organisms = study.Tables |> Seq.collect getOrganisms |> Seq.toList
//         Genotypes = study.Tables |> Seq.collect getGenotype |> Seq.toList
//         BiologicalReplicateCount = study.Tables |> Seq.map getReplicates |> Seq.max
//         TimepointCount = study.Tables |> Seq.map getTimepoints |> Seq.filter (fun x -> x > 0) |> Seq.length |> Some //|> Seq.toList
//         SampleCount = Some (study.Tables |> getSampleCount)
//         Parameters = study.Tables |> Seq.map ArcTable.getAllParameters |> Seq.concat |> Seq.distinct |> Seq.toList
//         Factors = study.Tables |> Seq.map ArcTable.getAllFactors |> Seq.concat |> Seq.distinct |> Seq.toList
//         AssociatedAssays = associatedAssaysForStudy investigation study
//         PreviousAssayIdentifiers = getPreviousAssayIdsForStudy study investigation
//         PreviousStudyIdentifiers = getPreviousStudyIdsForStudy study investigation
//         FollowingAssayIdentifiers = getFollowingAssayIdsForStudy study investigation
//         FollowingStudyIdentifiers = getFollowingStudyIdsForStudy study investigation
//         DataFileCount = Some (study.Tables |> Seq.map getDataFiles |> Seq.sum)
//     } 

//     let getAssayOverview (investigation:ArcInvestigation) (assay:ArcAssay) = {  
//         AssayIdentifier = assay.Identifier
//         AssayTitle = assay.Title
//         AssayDescription = assay.Description 
//         MeasurementType = assay.MeasurementType
//         MeasurementDevice = assay.Tables |> Seq.collect getMeasurementDevice |> Seq.toList    
//         TechnologyType = assay.TechnologyType                                                    
//         TableCount = Some assay.TableCount                        
//         TableNames = assay.TableNames
//         Characteristics = assay.Tables |> Seq.map ArcTable.getAllCharacteristics |> Seq.concat |> Seq.distinct |> Seq.toList
//         SampleCount = Some (assay.Tables |> getSampleCount)                                                                                                          
//         Parameters =  assay.Tables |> Seq.map ArcTable.getAllParameters |> Seq.concat |> Seq.distinct |> Seq.toList
//         Factors = assay.Tables |> Seq.map ArcTable.getAllFactors |> Seq.concat |> Seq.distinct |> Seq.toList
//         AssociatedStudies = associatedStudiesForAssay investigation assay
//         PreviousAssayIdentifiers = getPreviousAssayIdsForAssay assay investigation
//         PreviousStudyIdentifiers = getPreviousStudyIdsForAssay assay investigation
//         FollowingAssayIdentifiers = getFollowingAssayIdsForAssay assay investigation
//         FollowingStudyIdentifiers = getFollowingStudyIdsForAssay assay investigation
//         DataFileCount = Some (assay.Tables |> Seq.map getDataFiles |> Seq.sum)
//     }

//     let getOntologyForPrompt (table:ArcTable)  =
//         let tabChars = 
//             if not (List.isEmpty (getAllCharacteristics table)) then
//                 $"Characteristics: ({getAllOntologyInfos (getOntologyHeaders(getAllCharacteristics table)) table})"
//             else ""
//         let tabParams = 
//             if not (List.isEmpty (getAllParameters table)) then
//                 $"Parameters: ({getAllOntologyInfos (getOntologyHeaders(getAllParameters table)) table})"
//             else ""
//         let tabFacs = 
//             if not (List.isEmpty (getAllFactors table)) then
//                 $"Factors: {getAllOntologyInfos (getOntologyHeaders(getAllFactors table)) table})"
//             else ""
//         String.Join("\n", tabChars, tabParams, tabFacs)


//     let getBasicStudyPrompt (investigation:ArcInvestigation) = 
//         investigation.Studies
//         |> Seq.filter (fun (s:ArcStudy) -> s.TableCount <> 0)
//         |> Seq.map(fun (study:ArcStudy) ->
//             study
//             |> Seq.map(fun (table:ArcTable) ->  
//                 let ontologies = getOntologyForPrompt table
//                 if not (String.IsNullOrWhiteSpace(ontologies)) then 
//                     let studyID = $"## Study_{study.Identifier}_{table.Name} \n"
//                     String.Join("\n", studyID, ontologies)
//                 else ""
//                 )
//             |> Seq.filter(fun s -> not (String.IsNullOrWhiteSpace(s)))
//             |> String.concat "\n"
//         )
//         |> String.concat "\n"

//     let getEnhancedStudyPrompt (investigation:ArcInvestigation) = 
//         investigation.Studies
//         |> Seq.filter (fun (s:ArcStudy) -> s.TableCount <> 0)
//         |> Seq.map(fun (study:ArcStudy) ->
//             let basePrompt =
//                 study
//                 |> Seq.map(fun (table:ArcTable) -> 
//                     let ontologies = getOntologyForPrompt table
//                     if not (String.IsNullOrWhiteSpace(ontologies)) then 
//                         let studyID = $"\n ## Study_{study.Identifier}_{table.Name}"
//                         String.Join("\n", studyID, ontologies)
//                     else ""
//                     )
//                 |> Seq.filter(fun s -> not (String.IsNullOrWhiteSpace(s)))
//                 |> String.concat "\n"
//             let studyDescription = 
//                     if study.Description.IsSome then 
//                         $"### Description {study.Identifier}: {study.Description.Value} \n"
//                     else ""
//             let addDetails = 
//                     $"### Additional Details {study.Identifier} \n {createStudyAdditionalDetails (getStudyOverview investigation study)}"
//             String.Join("\n", basePrompt, studyDescription, addDetails)
//         )
//         |> String.concat "\n"


//     let getBasicAssayPrompt (investigation:ArcInvestigation) = 
//         investigation.Assays
//         |> Seq.filter (fun (a:ArcAssay) -> a.TableCount <> 0)
//         |> Seq.map(fun (assay:ArcAssay) ->
//             assay
//             |> Seq.map(fun (table:ArcTable) ->  
//                 let ontologies = getOntologyForPrompt table
//                 if not (String.IsNullOrWhiteSpace(ontologies)) then 
//                     let assayID = $"## Assay_{assay.Identifier}_{table.Name} \n"
//                     String.Join("\n", assayID, ontologies)
//                 else ""
//                 )
//             |> Seq.filter(fun s -> not (String.IsNullOrWhiteSpace(s)))
//             |> String.concat "\n"
//         )
//         |> String.concat "\n"

//     let getEnhancedAssayPrompt (investigation:ArcInvestigation) = 
//         investigation.Assays
//         |> Seq.filter (fun (a:ArcAssay) -> a.TableCount <> 0)
//         |> Seq.map(fun (assay:ArcAssay) ->

//             let basePrompt =
//                 assay
//                 |> Seq.map(fun (table:ArcTable) -> 
//                     let ontologies = getOntologyForPrompt table
//                     if not (String.IsNullOrWhiteSpace(ontologies)) then 
//                         let assayID = $"\n ## Assay_{assay.Identifier}_{table.Name}"
//                         String.Join("\n", assayID, ontologies)
//                     else ""
//                     )
//                 |> Seq.filter(fun s -> not (String.IsNullOrWhiteSpace(s)))
//                 |> String.concat "\n"
//             let assayDescription = 
//                     if assay.Description.IsSome then 
//                         $"### Description {assay.Identifier}: {assay.Description.Value} \n"
//                     else ""
//             let addDetails = 
//                     $"### Additional Details {assay.Identifier} \n {createAssayAdditionalDetails (getAssayOverview investigation assay)}"
//             String.Join("\n", basePrompt, assayDescription, addDetails)
//         )
//         |> String.concat "\n"


// open Promptdesign2

//TestARcs....


// let basicPrompt (investigation:ArcInvestigation) = 
//     let sb = StringBuilder()
//     sb.AppendLine("Summarize the biological and experimental objectives of the studies and assays below. Extract the key experimental factors and their values from the metadata in order to construct a 2-3 sentence summary of the objectives.") |> ignore
//     sb.AppendLine($"The found studies are listed with their respective id consisting of Study_Identifier_TableName alongside the ontology information here: {getBasicStudyPrompt investigation}") |> ignore
//     sb.AppendLine($"The found assay are listed respective table name alongside the ontology information here: {getBasicAssayPrompt investigation} ") |> ignore
//     sb.AppendLine("Summarize in a short paragraph suitable for a methods section.")

// let enhancedPrompt (investigation:ArcInvestigation) =
//     let sb = StringBuilder()
//     sb.AppendLine("Summarize the biological and experimental objectives of the studies and assays below. Extract the key experimental factors and their values from the metadata in order to construct a 2-3 sentence summary of the objectives.") |> ignore
//     sb.AppendLine($"The found studies are listed with their respective id consisting of Study_Identifier_TableName alongside the ontology information here: {getEnhancedStudyPrompt investigation}") |> ignore
//     sb.AppendLine($"This workflow highlights with --> which StudyIDs have matching output to input of another TableID: \n {allStudyIDNodes (getAllTableNodes investigation)}") |> ignore
//     sb.AppendLine($"The found assay are listed respective table name alongside the ontology information here: {getEnhancedAssayPrompt investigation} ") |> ignore
//     sb.AppendLine($"This workflow highlights with --> which AssayIDs have matching output to input of another TableID: \n {allAssayIDNodes (getAllTableNodes investigation)}") |> ignore
//     sb.AppendLine("Summarize in a short paragraph suitable for a methods section.")


// let hybridPrompt(investigation:ArcInvestigation) = 
//     let sb = StringBuilder()
//     sb.AppendLine($"The following overview table contains metadata of the entire investigation: {createOverviewTable (getTopLevelMetadata investigation)}") |> ignore
//     sb.AppendLine($"The found studies are listed with their respective id consisting of Study_Identifier_TableName alongside the ontology information here: {getEnhancedStudyPrompt investigation}") |> ignore
//     sb.AppendLine($"This workflow highlights with --> which StudyIDs have matching output to input of another TableID: \n {allStudyIDNodes (getAllTableNodes investigation)} \n") |> ignore
//     sb.AppendLine($"The found assays are listed with their respective id consisting of Assay_Identifier_TableName alongside the ontology information here: {getEnhancedAssayPrompt investigation} ") |> ignore // not uniform to studys
//     sb.AppendLine($"This workflow highlights with --> which AssayIDs have matching output to input of another TableID: \n {allAssayIDNodes (getAllTableNodes investigation)} \n") |> ignore

//     sb.AppendLine("## INSTRUCTIONS (CHAIN-OF-THOUGHT)") |> ignore
//     sb.AppendLine("1. **Identify the objectives based on key variables such as factors of the study**") |> ignore
//     sb.AppendLine("2. **Infer the experimental design**: How are different conditions being compared?") |> ignore
//     sb.AppendLine("3. **Map the experimental workflow** using the `-->` structure from Study to Assay.") |> ignore
//     sb.AppendLine("4. **Summarize key methods used** in harvesting, extraction, and measurement stages.") |> ignore
//     sb.AppendLine("5. Combine all insights into a cohesive summary structured in two paragraphs:") |> ignore
//     sb.AppendLine("- **Paragraph 1 (2-3 sentences)**: State the main experimental objectives from 1.") |> ignore 
//     sb.AppendLine("- **Paragraph 2 (5-7 sentences)**: Describe the experimental design and methodology in appropriate scientific language") |> ignore
//     sb.AppendLine("6. Use any domain-specific vocabulary from the Description fields to improve specificity if available. \n") |> ignore

//     sb.AppendLine("## CONSTRAINTS") |> ignore
//     sb.AppendLine("- Use formal scientific language appropriate for the methods section or abstract.") |> ignore
//     sb.AppendLine("- Keep the output concise but specific.") |> ignore
//     sb.AppendLine("- Emphasize treatment factors, organism studied, and downstream assays.") |> ignore
//     sb.AppendLine("- Avoid simply listing assays or instruments unless relevant to the objective or method.") |> ignore
//     sb.AppendLine("- Weight content as follows: **[IMPORTANT] Experimental objectives → [MEDIUM] Methodology → [LESS] Equipment details**.") 

//TestInstances



// Next steps:
// Emulate last output found in Notes
// Evaluate for other two sample arcs



// ## CHAIN-OF-THOUGHT
// 1. Identify experimental objectives.
// 2. Describe the organism, genotype, and treatments.
// 3. Map the experimental design (replicates, factors, parameters).
// 4. Outline the measurement workflow from sample to data.
// 5. Use this structure to write a coherent summary (5–7 sentences).
// - Use any domain-specific vocabulary in the Description fields.
// - Integrate detailed vocabulary to improve specificity.



// let newPrompt = 
//     "You are a scientific writing assistant helping to generate the Methods section of a biology research paper. Use the metadata below to summarize the biological objectives and experimental design in a concise paragraph. 
//     Focus on what was tested, under which conditions, and how. If relationships between assays exist, highlight them as part of a coherent experimental pipeline. Use natural, domain-appropriate language and infer meaning where metadata is incomplete or implicit.
//     Factors/Key experimental variables include: 
//     "
// consider including study/assay description if available
// consider adding format specifiers to interpolated strings for type safety
// Represent DOI as Badge? Applicable?
// Validation package for readme?
// Publish on nuget
// Flag in the Config File Citationstyle for Publication for Citable Formats
// Include organism ontology into field for getOrganisms
// Rework Publication section and make fields optional






// // Issue: when the previous markdown already contains sections named equally as the generated ones like say Publication the hyperlink works accordingly for the first and not the second
// // Check if the order in Table of Contents machtes the actual order here assays are swapped in testing
// // include StyleFormats into cli arguments
// // something to consider: currently the multiple workflows are shortcut at some point with ...]


// let getStudyPrompt (investigation:ArcInvestigation) =
//         investigation.Studies
//         |> Seq.filter (fun (s:ArcStudy) -> s.TableCount <> 0)
//         |> Seq.map(fun (study:ArcStudy) ->
//             study
//             |> Seq.map(fun (table:ArcTable) -> 
//                 let studyID = $"## Study_{study.Identifier}_{table.Name} \n"
//                 let studyDescription= 
//                     if study.Description.IsSome then 
//                         $"### Description: {study.Description.Value} \n"
//                     else ""
//                 let tabChars = 
//                     if not (List.isEmpty (getAllCharacteristics table)) then
//                         $"Characteristics: ({getAllOntologyInfos (getOntologyHeaders(getAllCharacteristics table)) table})"
//                     else ""
//                 let tabParams = 
//                     if not (List.isEmpty (getAllParameters table)) then
//                         $"Parameters: ({getAllOntologyInfos (getOntologyHeaders(getAllParameters table)) table})"
//                     else ""
//                 let tabFacs = 
//                     if not (List.isEmpty (getAllFactors table)) then
//                         $"Factors: {getAllOntologyInfos (getOntologyHeaders(getAllFactors table)) table})"
//                     else ""
//                 let addDetails = 
//                     $"{createStudyAdditionalDetails (getStudyOverview investigation study)}"
//                 String.Join("\n",studyDescription, studyID, tabChars, tabParams, tabFacs, addDetails)
//                 )
//             |> String.concat "\n"
//         )
//         |> String.concat "\n"


// let getAssayPrompt (investigation:ArcInvestigation) =
//         investigation.Assays
//         |> Seq.filter (fun (a:ArcAssay) -> a.TableCount <> 0)
//         |> Seq.map(fun (assay:ArcAssay) ->
//             assay
//             |> Seq.map(fun (table:ArcTable) -> 
//                 let assayID = $"## Assay_{assay.Identifier}_{table.Name} \n"
//                 let assayDescription = 
//                     if assay.Description.IsSome then 
//                         $"### Description: {assay.Description.Value} \n"
//                     else ""
//                 let tabChars = 
//                     if not (List.isEmpty (getAllCharacteristics table)) then
//                         $"Characteristics: ({getAllOntologyInfos (getOntologyHeaders(getAllCharacteristics table)) table})"
//                     else ""
//                 let tabParams = 
//                     if not (List.isEmpty (getAllParameters table)) then
//                         $"Parameters: ({getAllOntologyInfos (getOntologyHeaders(getAllParameters table)) table})"
//                     else ""
//                 let tabFacs = 
//                     if not (List.isEmpty (getAllFactors table)) then
//                         $"Factors: {getAllOntologyInfos (getOntologyHeaders(getAllFactors table)) table})"
//                     else ""
//                 let addDetails = 
//                     $"{createAssayAdditionalDetails (getAssayOverview investigation assay)}"
//                 String.Join("\n", assayDescription, assayID, tabChars, tabParams, tabFacs, addDetails)
//                 )
//             |> String.concat "\n"
//         )
//         |> String.concat "\n"
