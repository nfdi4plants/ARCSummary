namespace ARCSummary

open ARCtrl
open System
open System.IO
open TemplateHelpers
open ARCInstances
open Template
open SummaryTypes
open ConfigFileTypes


module READMEAutomation = // better name 

    let createMarkdownSummary (sections:Section list) (investigation:ArcInvestigation) : string  = 
        let mutable studiesHeaderSet = false              
        let mutable assayHeaderSet = false
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
            | Investigation InvestigationSection.Title -> createInvTitle tlm 
            | Investigation InvestigationSection.Description -> createInvDescription tlm 
            | Investigation Contacts -> createContactsSection tlm 
            | Investigation Publication -> createPublicationsSection tlm 
            | TOC -> TableOfContents.createTOC(sections, tlm ,assayOVs, studyOVs)
            | ISAGraph -> createRelationshipGraph tlm investigation assayOVs studyOVs
            | OverviewTable -> createOverviewTable tlm 
            | Studies _ -> 
                if not studiesHeaderSet then         
                    studiesHeaderSet <- true
                    studyOVs
                    |> List.ofSeq
                    |> List.map (fun (sOV:StudyOverview) ->
                        let id = createStudySectionId sOV
                        let studySubSecs =
                            orderedStudySections
                            |> List.map (fun subSec ->
                                match subSec with
                                | StudySection.Title -> createStudyTitle sOV
                                | StudySection.Description -> createStudyDescription sOV
                                | StudySection.AdditionalDetails -> createStudyAdditionalDetails sOV
                                | StudySection.AnnotationHeaders -> createStudyAnnotationHeaders sOV
                            )
                            |> String.concat "\n"
                        [id;studySubSecs]
                        |> String.concat "\n"
                    )
                    |> String.concat "\n\n"
                else ""
            | Assays _ ->
                if not assayHeaderSet then
                    assayHeaderSet <- true 
                    assayOVs
                    |> List.ofSeq
                    |> List.map (fun (aOV:AssayOverview) ->
                        let id = createAssaySectionId aOV
                        let assaySubSecs =
                            orderedAssaySections
                            |> List.map (fun subSec ->
                                match subSec with
                                | AssaySection.Title -> createAssayTitle aOV
                                | AssaySection.Description -> createAssayDescription aOV
                                | AssaySection.AdditionalDetails -> createAssayAdditionalDetails aOV
                                | AssaySection.AnnotationHeaders -> createAssayAnnotationHeaders aOV
                            )
                            |> String.concat "\n"
                        [id;assaySubSecs]
                        |> String.concat "\n"

                    )
                    |> String.concat "\n\n"
                else ""
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



