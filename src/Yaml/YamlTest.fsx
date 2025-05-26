//namespace ARCSummary



// #r "nuget: YAMLicious, 0.0.3"
// #r "nuget: ARCtrl, 3.0.0-alpha.3"
// #r "nuget: Fable.Core, 4.5.0"
// #r "../Core/bin/Debug/netstandard2.0/Core.dll"
// #r "../Yaml/bin/Debug/net9.0/Yaml.dll"


// open ARCtrl.Yaml
// open System.IO
// open YAMLicious 
// open YAMLicious.YAMLiciousTypes
// open System.IO
// open YAMLicious.Decode.Helper    
// open Fable.Core 
// open ARCtrl
// open System
// open System.IO
// open System.Text
// open ARCSummary
// open TemplateHelpers
// open ARCInstances
// open Template
// open ConfigFileTypes
// open Formating
// open StringHelper
// open ARCSummary.SummaryTypes

// let yamlContentEx :string =
//     File.ReadAllText("../.arc/arc-summary.yml") //

// let parsedEx = Reader.read yamlContentEx


// module newTemplates =

    // //Part 1: Title, Description, Contacts and Publication of Investigation
    // let createInvTitle (tlm:TopLevelMetadata) : string =
    //     let sb = StringBuilder()

    //     if tlm.Title.IsSome then 
    //         sb.AppendLine($"# {tlm.Title.Value}") |> ignore
    //     else sb.AppendLine("# Please add a valid title for your ArcInvestigation") |> ignore
    //     sb.ToString()

    // let createInvDescription (tlm:TopLevelMetadata) : string =
    //     let sb = StringBuilder()
    //     if tlm.Description.IsSome then
    //         sb.AppendLine($"### Description\n{tlm.Description.Value}") |> ignore
    //     else sb.AppendLine("### Please add a valid description to your ArcInvestigation") |> ignore

    //     sb.ToString()



    // //Part 3: Studies: Description, Additional details, Annotation headers
    // let createStudyIntro (sOV:StudyOverview) : string =
    //     let sb = StringBuilder()
    //     sb.AppendLine($"## Study: _{sOV.StudyIdentifier}_") |> ignore
    //     if sOV.StudyDescription.IsSome then sb.AppendLine($"### Description\n{sOV.StudyDescription.Value}") |> ignore
    //     sb.ToString()

    // let createStudyAdditionalDetails (sOV:StudyOverview) : string =
    //     let sb = StringBuilder()

    //     sb.AppendLine("### Additional details") |> ignore
    //     sb.AppendLine("| Meta Data | Description |") |> ignore
    //     sb.AppendLine("| --------- | ----------- |") |> ignore
    //     sb.AppendLine($"| Table Count | {sOV.TableCount.Value} |") |> ignore
    //     let tableNamesString = String.Join(" , ", sOV.TableNames)
    //     sb.AppendLine($"| Table Names | {tableNamesString} |") |> ignore
    //     sb.AppendLine($"| Sample Count | {sOV.SampleCount.Value} |") |> ignore
    //     sb.AppendLine($"| Data File Count | {sOV.DataFileCount.Value} |") |> ignore
    //     let laString = String.Join(" , ", sOV.AssociatedAssays)
    //     sb.AppendLine($"| Associated assays | {laString} |") |> ignore
    //     if sOV.Organisms.IsEmpty = false then
    //         let organism = String.Join(" , ", (sOV.Organisms |> List.map (fun oa -> oa.NameText)))
    //         sb.AppendLine($"| Organisms | _{organism}_ |") |> ignore
    //     if sOV.Genotypes.IsEmpty = false then
    //         let genotype = String.Join(" , ", (sOV.Genotypes |> List.map (fun oa -> oa.NameText)))
    //         sb.AppendLine($"| Genotype | _{genotype}_ |") |> ignore
    //     sb.AppendLine($"| Biological replicates | {sOV.BiologicalReplicateCount} |") |> ignore
    //     if sOV.TimepointCount.Value > 0 = true then 
    //         let tps = String.Join(" , ", sOV.TimepointCount.Value)
    //         sb.AppendLine($"| Time points | {tps} |") |> ignore
    //     sb.ToString()

    // let createStudyAnnotationHeaders (sOV:StudyOverview) : string =
    //     let sb = StringBuilder()
    //     sb.AppendLine("### Annotation headers") |> ignore
    //     if sOV.Characteristics.IsEmpty = false then 
    //         let charString = getHyperlinks sOV.Characteristics
    //         sb.AppendLine($"\n**Characteristics**: {charString}") |> ignore
    //     if sOV.Parameters.IsEmpty = false then 
    //         let paramsString = getHyperlinks sOV.Parameters
    //         sb.AppendLine($"\n**Parameters**: {paramsString}") |> ignore
    //     if sOV.Factors.IsEmpty = false then 
    //         let factString = getHyperlinks sOV.Factors    
    //         sb.AppendLine($"\n**Factors**: {factString}") |> ignore
    //     sb.ToString()

    // // Part 4: Assays: Description, Additional details, Annotation headers
    // let createAssayIntro (aOV:AssayOverview) : string =
    //     let sb = StringBuilder()
    //     sb.AppendLine($"## Assay: _{aOV.AssayIdentifier}_") |> ignore
    //     if aOV.AssayDescription.IsSome then sb.AppendLine($"### Description\n{aOV.AssayDescription.Value}") |> ignore
    //     sb.ToString()


    // let createAssayAdditionalDetails (aOV:AssayOverview) : string =
    //     let sb = StringBuilder()
    //     sb.AppendLine("### Additional details") |> ignore 
    //     sb.AppendLine("| Meta Data | Description |") |> ignore
    //     sb.AppendLine("| --------- | ----------- |") |> ignore
    //     if aOV.MeasurementType.IsSome = true then // change to = true
    //         sb.AppendLine($"| Measurement Type | {aOV.MeasurementType.Value.NameText} |") |> ignore 
    //     if aOV.MeasurementDevice.IsEmpty = false then 
    //         sb.AppendLine($"| Measurement Device | {aOV.MeasurementDevice.Head.Name.Value} |") |> ignore 
    //     if aOV.TechnologyType.IsSome = true then
    //         sb.AppendLine($"| Technology Type | {aOV.TechnologyType.Value.NameText} |") |> ignore
    //     sb.AppendLine($"| Table Count | {aOV.TableCount.Value} |") |> ignore
    //     let tableNamesString = String.Join(" , ", aOV.TableNames)
    //     sb.AppendLine($"| Table Names | {tableNamesString} |") |> ignore
    //     sb.AppendLine($"| Sample Count | {aOV.SampleCount.Value} |") |> ignore
    //     sb.AppendLine($"| Data File Count | {aOV.DataFileCount.Value} |") |> ignore
    //     let lsString = String.Join(" , ", aOV.AssociatedStudies)
    //     sb.AppendLine($"| Associated studies | {lsString} |") |> ignore
    //     sb.ToString()

    // let createAssayAnnotationHeaders (aOV:AssayOverview) : string =
    //     let sb = StringBuilder()
    //     sb.AppendLine("### Annotation headers") |> ignore
    //     if aOV.Characteristics.IsEmpty = false then 
    //         let charString = getHyperlinks aOV.Characteristics
    //         sb.AppendLine($"\n**Characteristics**: {charString}") |> ignore
    //     if aOV.Parameters.IsEmpty = false then 
    //         let paramsString = getHyperlinks aOV.Parameters
    //         sb.AppendLine($"\n**Parameters**: {paramsString}") |> ignore
    //     if aOV.Factors.IsEmpty = false then 
    //         let factString = getHyperlinks aOV.Factors    
    //         sb.AppendLine($"\n**Factors**: {factString}") |> ignore
    //     sb.ToString()


    // // Part 2: Relationship Graph & TLM data , Publications & Contacts if given 
    // let createRelationshipGraph (tlm:TopLevelMetadata) (investigation:ArcInvestigation) (assayOVs:seq<AssayOverview>) (studyOVs:seq<StudyOverview>) : string =
    //     let sb = StringBuilder()

    //     let relationshipGraph = getRelationshipGraph investigation assayOVs studyOVs
    //     sb.AppendLine($"## Relationships between Assays and Studies \n {relationshipGraph} ")|> ignore
    //     // sb.AppendLine("_This flowchart highlights the relationship between assays (highlighted in blue) and studies (highlighted in green)_") |> ignore
    //     sb.ToString()

    // let createOverviewTable (tlm:TopLevelMetadata) : string =
    //     let sb = StringBuilder()
        
    //     sb.AppendLine("### Overview Table") |> ignore
    //     sb.AppendLine("| Meta Data | Description |") |> ignore
    //     sb.AppendLine("| --------- | ----------- |") |> ignore
    //     if tlm.SubmissionDate.IsSome then
    //         sb.AppendLine($"| Submission Date | {tlm.SubmissionDate.Value} |") |> ignore
    //     else sb.AppendLine("| Submission Date  | tba |") |> ignore
    //     if tlm.PublicReleaseDate.IsSome then
    //         sb.AppendLine($"| Public Release Date | {tlm.PublicReleaseDate.Value} |") |> ignore
    //     else sb.AppendLine("| Public Release Date | tba |") |> ignore
    //     let studyIDs = String.Join(" , ", tlm.StudyIdentifiers)
    //     sb.AppendLine($"| Study identifiers | {studyIDs} |") |> ignore
    //     sb.AppendLine($"| Study Count | {tlm.StudyCount.Value} |") |> ignore
    //     let assayIDs = String.Join(" , ", tlm.AssayIdentifiers)
    //     sb.AppendLine($"| Assay identifiers | {assayIDs} |") |> ignore
    //     sb.AppendLine($"| Assay Count | {tlm.AssayCount.Value} |") |> ignore

    //     sb.ToString()


    //Table of Contents
    // create TOC with numbered list and bullet point for the options such as multiple assays 


    // let defaultOrder : Section list = 
    //     [
    //         Section.Investigation InvestigationSection.Title
    //         Section.Investigation InvestigationSection.Description
    //         Section.TOC
    //         Section.ISAGraph
    //         Section.OverviewTable
    //         Section.Investigation InvestigationSection.Contacts
    //         Section.Investigation InvestigationSection.Publication
    //         Section.Studies StudySection.Intro
    //         Section.Studies StudySection.AdditionalDetails
    //         Section.Studies StudySection.AnnotationHeaders       
    //         Section.Assays AssaySection.Intro
    //         Section.Assays AssaySection.AdditionalDetails
    //         Section.Assays AssaySection.AnnotationHeaders
    //     ]

    // type TableOfContents =


    //     static member createAnchor (prefix:string) (ids:seq<string>) =
    //             ids
    //             |> Seq.map (fun (id:string) ->
    //                 $"     - [{id}](#{prefix}-{id.ToLower()})")
    //             |> String.concat "\n"

    //     static member createTOC(sections : Section list, tlm : TopLevelMetadata , ?assayOVs : seq<AssayOverview>, ?studyOVs : seq<StudyOverview>) =    
    //         let mutable studiesHeaderSet = false              
    //         let mutable assayHeaderSet = false

    //         let studyOVs = Option.defaultValue Seq.empty studyOVs
    //         let assayOVs = Option.defaultValue Seq.empty assayOVs

    //         let sb = StringBuilder()
    //         sb.AppendLine("## Table of Contents \n") |> ignore

    //         for sec in sections do
    //             match sec with 
    //             | Section.ISAGraph -> // no check if Empty 
    //                 sb.AppendLine("- [ISA Graph](#relationships-between-assays-and-studies)") |> ignore
    //             | Section.OverviewTable -> // no check if Empty 
    //                 sb.AppendLine("- [Overview Table](#overview-table)") |> ignore

    //             | Section.Investigation InvestigationSection.Contacts when not (tlm.Contacts.IsEmpty) ->
    //                 sb.AppendLine("- [Contacts](#contacts)") |> ignore
    //             | Section.Investigation InvestigationSection.Publication when not (tlm.Publications.IsEmpty) ->
    //                 sb.AppendLine("- [Publication](#publication)") |> ignore     
    //             | Section.Studies subSection when not (Seq.isEmpty studyOVs) ->
    //                 if not studiesHeaderSet then 
    //                     sb.AppendLine("- Studies \n ") |> ignore
    //                     studiesHeaderSet <- true
    //                     sb.AppendLine(TableOfContents.createAnchor "study" (studyOVs |> Seq.map (fun (sOV:StudyOverview) -> sOV.StudyIdentifier))) |> ignore

    //             | Section.Assays subSection when not (Seq.isEmpty assayOVs) -> 
    //                 if not assayHeaderSet then 
    //                     sb.AppendLine("- Assays \n ") |> ignore
    //                     assayHeaderSet <- true
    //                     sb.AppendLine(TableOfContents.createAnchor "assay" (assayOVs |> Seq.map (fun (aOV:AssayOverview) -> aOV.AssayIdentifier))) |> ignore            
    //             | Section.Investigation InvestigationSection.Title 
    //             | Section.Investigation InvestigationSection.Description
    //             | Section.TOC -> ()
    //         sb.ToString()


// open newTemplates

// let arcPathRU = "/pathToARC"
// let sampleARC = ARC.load(arcPathRU)

// module SectionCompiler =


//     let createMarkdownSummary (sections : Section list) (investigation:ArcInvestigation) : string  = 
//         let tlm : TopLevelMetadata = getTopLevelMetadata investigation
//         let studyOVs : StudyOverview seq =
//             investigation.Studies
//             |> Seq.filter (fun (s:ArcStudy) -> s.TableCount <> 0)
//             |> Seq.map (getStudyOverview investigation)          

//         let assayOVs : AssayOverview seq =
//             investigation.Assays
//             |> Seq.filter (fun (a:ArcAssay) -> a.TableCount <> 0)
//             |> Seq.map (fun (a:ArcAssay) -> getAssayOverview investigation a)

//         let orderedStudySections =
//             sections
//             |> List.choose (function Section.Studies s -> Some s | _ -> None)

//         let orderedAssaySections =
//             sections
//             |> List.choose (function Section.Assays s -> Some s | _ -> None)

//         sections 
//         |> Seq.map (fun (sec:Section) ->
//             match sec with 
//             | Investigation Title -> createInvTitle tlm 
//             | Investigation Description -> createInvDescription tlm 
//             | Investigation Contacts -> createContactsSection tlm 
//             | Investigation Publication -> createPublicationsSection tlm 
//             | TOC -> TableOfContents.createTOC(sections, tlm ,assayOVs, studyOVs)
//             | ISAGraph -> createRelationshipGraph tlm investigation assayOVs studyOVs
//             | OverviewTable -> createOverviewTable tlm 
//             | Studies Intro -> // Other Study/Assay sections are handled in the 'Intro' match case. No-op here.
//                 studyOVs
//                 |> List.ofSeq
//                 |> List.map (fun (sOV:StudyOverview) ->
//                     orderedStudySections
//                     |> List.map (fun subSec ->
//                         match subSec with
//                         | StudySection.Intro -> createStudyIntro sOV
//                         | StudySection.AdditionalDetails -> createStudyAdditionalDetails sOV
//                         | StudySection.AnnotationHeaders -> createStudyAnnotationHeaders sOV
//                     )
//                     |> String.concat "\n"
//                 )
//                 |> String.concat "\n\n"
//             | Assays AssaySection.Intro -> // Same here
//                 assayOVs
//                 |> List.ofSeq
//                 |> List.map (fun (aOV:AssayOverview) ->
//                     orderedAssaySections
//                     |> List.map (fun subSec ->
//                         match subSec with
//                         | AssaySection.Intro -> createAssayIntro aOV
//                         | AssaySection.AdditionalDetails -> createAssayAdditionalDetails aOV
//                         | AssaySection.AnnotationHeaders -> createAssayAnnotationHeaders aOV
//                     )
//                     |> String.concat "\n"
//                 )
//                 |> String.concat "\n\n"
//             | _ -> ""
//         )
//         |> Seq.filter (fun s -> not (System.String.IsNullOrWhiteSpace s))
//         |> String.concat "\n\n"


//     /// function that handles creation of appended markdown, checks if content is already or in need of an update
//     let updateMarkdownContent (sections:Section list) (preexistingMD:string) (investigation:ArcInvestigation) : string = 
//         let automatedMD  = createMarkdownSummary sections investigation
//         let startMarker = "<!--- Start of automated section -->"
//         let endMarker = "<!--- End of automated section -->"
//         if preexistingMD.Contains(startMarker) && preexistingMD.Contains(endMarker) then 
//             let startIndex = preexistingMD.IndexOf(startMarker) + startMarker.Length
//             let endIndex = preexistingMD.IndexOf(endMarker)
//             let currentAutomatedContent = 
//                 preexistingMD.Substring(startIndex, endIndex - startIndex).Trim()
//             if currentAutomatedContent = automatedMD then
//                 preexistingMD
//             else
//                 let updatedContent =
//                     preexistingMD.Substring(0, startIndex) +
//                     $"\n{automatedMD}\n" +
//                     preexistingMD.Substring(endIndex)
//                 updatedContent
//         else String.Join("\n\n", [preexistingMD;startMarker;automatedMD;endMarker])
    
//     let updateMarkdownFile (sections:Section list) (mdFileName: string) (investigation: ArcInvestigation) = 
//         if File.Exists(mdFileName) then
//             let existingContent = File.ReadAllText(mdFileName)
//             let updatedMD = updateMarkdownContent sections existingContent investigation
//             if existingContent <> updatedMD then
//                 File.WriteAllText(mdFileName, updatedMD)
//             else
//                 printfn "File is already updated"
//         else
//             let automatedMD  = createMarkdownSummary sections  investigation
//             System.IO.FileInfo(mdFileName).Directory.Create() |> ignore
//             File.WriteAllText(mdFileName, automatedMD)

//     //Wrapper for correct path name (need to check arcPath for correct signature //)
//     let updateREADME (sections:Section list) (arcPath:string) (investigation:ArcInvestigation) =
//         let readmePath = System.IO.Path.Combine(arcPath, "README.md")
//         updateMarkdownFile sections readmePath investigation  
//         readmePath      

// open SectionCompiler 

// updateREADME defaultOrder arcPathRU sampleARC

// Issue: when the previous markdown already contains sections named equally as the generated ones like say Publication the hyperlink works accordingly for the first and not the second
// Check if the order in Table of Contents machtes the actual order here assays are swapped in testing
// include StyleFormats into cli arguments







