namespace ARCSummary

open ARCtrl
open System
open System.IO
open TemplateHelpers
open ARCInstances
open Template
open SummaryTypes
open ConfigFileTypes


module SummaryStyles =
    let defaultOrder : Section list = 
        [
            Section.Investigation InvestigationSection.Title
            Section.Investigation InvestigationSection.Description
            Section.TOC
            Section.ISAGraph
            Section.OverviewTable
            Section.Investigation InvestigationSection.Contacts
            Section.Investigation InvestigationSection.Publication
            Section.Studies StudySection.Intro
            Section.Studies StudySection.AdditionalDetails
            Section.Studies StudySection.AnnotationHeaders       
            Section.Assays AssaySection.Intro
            Section.Assays AssaySection.AdditionalDetails
            Section.Assays AssaySection.AnnotationHeaders
        ]
    
    let publicationStyle : Section list = // Title, Description, Contacts, Publication (check if available title with publication title)
        [
            Section.Investigation InvestigationSection.Title
            Section.Investigation InvestigationSection.Description
            Section.Investigation InvestigationSection.Contacts
            Section.Investigation InvestigationSection.Publication
        ]

module READMEAutomation = // better name 

    let createMarkdownSummary (sections:Section list) (investigation:ArcInvestigation) : string  = 
        let tlm : TopLevelMetadata = getTopLevelMetadata investigation
        let studyOVs : StudyOverview seq =
            investigation.Studies
            |> Seq.filter (fun (s:ArcStudy) -> s.TableCount <> 0)
            |> Seq.map (getStudyOverview investigation)          

        let assayOVs : AssayOverview seq =
            investigation.Assays
            |> Seq.filter (fun (a:ArcAssay) -> a.TableCount <> 0)
            |> Seq.map (fun (a:ArcAssay) -> getAssayOverview investigation a)

        let orderedStudySections : StudySection list =
            sections
            |> List.choose (function Section.Studies (s:StudySection) -> Some s | _ -> None)

        let orderedAssaySections : AssaySection list =
            sections
            |> List.choose (function Section.Assays (a:AssaySection) -> Some a | _ -> None)

        sections 
        |> Seq.map (fun (sec:Section) ->
            match sec with 
            | Investigation Title -> createInvTitle tlm 
            | Investigation Description -> createInvDescription tlm 
            | Investigation Contacts -> createContactsSection tlm 
            | Investigation Publication -> createPublicationsSection tlm 
            | TOC -> TableOfContents.createTOC(sections, tlm ,assayOVs, studyOVs)
            | ISAGraph -> createRelationshipGraph tlm investigation assayOVs studyOVs
            | OverviewTable -> createOverviewTable tlm 
            | Studies Intro -> // Other Study/Assay sections are handled in the 'Intro' match case. No-op here.
                studyOVs
                |> List.ofSeq
                |> List.map (fun (sOV:StudyOverview) ->
                    orderedStudySections
                    |> List.map (fun subSec ->
                        match subSec with
                        | StudySection.Intro -> createStudyIntro sOV
                        | StudySection.AdditionalDetails -> createStudyAdditionalDetails sOV
                        | StudySection.AnnotationHeaders -> createStudyAnnotationHeaders sOV
                    )
                    |> String.concat "\n"
                )
                |> String.concat "\n\n"
            | Assays AssaySection.Intro -> // Same here
                assayOVs
                |> List.ofSeq
                |> List.map (fun (aOV:AssayOverview) ->
                    orderedAssaySections
                    |> List.map (fun subSec ->
                        match subSec with
                        | AssaySection.Intro -> createAssayIntro aOV
                        | AssaySection.AdditionalDetails -> createAssayAdditionalDetails aOV
                        | AssaySection.AnnotationHeaders -> createAssayAnnotationHeaders aOV
                    )
                    |> String.concat "\n"
                )
                |> String.concat "\n\n"
            | _ -> ""
        )
        |> Seq.filter (fun s -> not (System.String.IsNullOrWhiteSpace s))
        |> String.concat "\n\n"



    let updateMarkdownContent (sections:Section list) (preexistingMD:string) (investigation:ArcInvestigation) : string = 
        let automatedMD  = createMarkdownSummary sections investigation
        let startMarker = "<!--- Start of automated section -->"
        let endMarker = "<!--- End of automated section -->"
        if preexistingMD.Contains(startMarker) && preexistingMD.Contains(endMarker) then 
            let startIndex = preexistingMD.IndexOf(startMarker) + startMarker.Length
            let endIndex = preexistingMD.IndexOf(endMarker)
            let currentAutomatedContent = 
                preexistingMD.Substring(startIndex, endIndex - startIndex).Trim()
            if currentAutomatedContent = automatedMD then
                preexistingMD
            else
                let updatedContent =
                    preexistingMD.Substring(0, startIndex) +
                    $"\n{automatedMD}\n" +
                    preexistingMD.Substring(endIndex)
                updatedContent
        else String.Join("\n\n", [preexistingMD;startMarker;automatedMD;endMarker])
    
    let updateMarkdownFile (sections:Section list) (mdFileName: string) (investigation: ArcInvestigation) = 
        if File.Exists(mdFileName) then
            let existingContent = File.ReadAllText(mdFileName)
            let updatedMD = updateMarkdownContent sections existingContent investigation
            if existingContent <> updatedMD then
                File.WriteAllText(mdFileName, updatedMD)
            else
                printfn "File is already updated"
        else
            let automatedMD  = createMarkdownSummary sections investigation
            System.IO.FileInfo(mdFileName).Directory.Create() |> ignore
            File.WriteAllText(mdFileName, automatedMD)

    //Wrapper for correct path name (need to check arcPath for correct signature //)
    let updateREADME (sections:Section list) (arcPath:string) (investigation:ArcInvestigation) =
        let readmePath = System.IO.Path.Combine(arcPath, "README.md")
        updateMarkdownFile sections readmePath investigation  
        readmePath      



