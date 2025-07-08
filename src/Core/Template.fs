namespace ARCSummary


open ARCtrl
open ARCtrl.Helper
open System
open System.Text
open Option
open ArcQuerying
open Formating
open TemplateHelpers
open SummaryTypes
open ConfigFileTypes
open StringHelper

module ARCInstances =
    let getTopLevelMetadata (selectISA:ArcInvestigation) : TopLevelMetadata = {
        Title = selectISA.Title ;
        Description = selectISA.Description ;
        SubmissionDate = selectISA.SubmissionDate ;
        PublicReleaseDate = selectISA.PublicReleaseDate ;
        Publications = selectISA.Publications |> Seq.toList ;
        Contacts = selectISA.Contacts |> Seq.toList ;
        AssayIdentifiers = selectISA.AssayIdentifiers |> Seq.toList ;
        AssayCount = Some selectISA.AssayCount ;
        StudyIdentifiers = selectISA.StudyIdentifiers |> Seq.toList ;
        StudyCount = Some selectISA.StudyCount
    }

    let getStudyOverview (investigation:ArcInvestigation) (study:ArcStudy) = {
        StudyIdentifier = study.Identifier
        StudyTitle = study.Title
        StudyDescription = study.Description
        TableCount = Some study.TableCount
        TableNames = study.TableNames
        Characteristics = study.Tables |> Seq.map ArcTable.getAllCharacteristics |> Seq.concat |> Seq.distinct |> Seq.toList
        Organisms = study.Tables |> ResizeArray.collect getOrganisms |> Seq.toList
        Genotypes = study.Tables |> ResizeArray.collect getGenotype |> Seq.toList
        BiologicalReplicateCount = study.Tables |> ResizeArray.map getReplicates |> Seq.max
        TimepointCount = study.Tables |> Seq.map getTimepoints |> Seq.filter (fun x -> x > 0) |> Seq.length |> Some //|> Seq.toList
        SampleCount = Some (study.Tables |> getSampleCount)
        Parameters = study.Tables |> Seq.map ArcTable.getAllParameters |> Seq.concat |> Seq.distinct |> Seq.toList
        Factors = study.Tables |> Seq.map ArcTable.getAllFactors |> Seq.concat |> Seq.distinct |> Seq.toList
        AssociatedAssays = associatedAssaysForStudy investigation study
        PreviousAssayIdentifiers = getPreviousAssayIdsForStudy study investigation
        PreviousStudyIdentifiers = getPreviousStudyIdsForStudy study investigation
        FollowingAssayIdentifiers = getFollowingAssayIdsForStudy study investigation
        FollowingStudyIdentifiers = getFollowingStudyIdsForStudy study investigation
        DataFileCount = Some (study.Tables |> ResizeArray.map getDataFiles |> Seq.sum)
    } 

    let getAssayOverview (investigation:ArcInvestigation) (assay:ArcAssay) = {  
        AssayIdentifier = assay.Identifier
        AssayTitle = assay.Title
        AssayDescription = assay.Description 
        MeasurementType = assay.MeasurementType
        MeasurementDevice = assay.Tables |> ResizeArray.collect getMeasurementDevice |> Seq.toList    
        TechnologyType = assay.TechnologyType                                                    
        TableCount = Some assay.TableCount                        
        TableNames = assay.TableNames
        Characteristics = assay.Tables |> Seq.map ArcTable.getAllCharacteristics |> Seq.concat |> Seq.distinct |> Seq.toList
        SampleCount = Some (assay.Tables |> getSampleCount)                                                                                                          
        Parameters =  assay.Tables |> Seq.map ArcTable.getAllParameters |> Seq.concat |> Seq.distinct |> Seq.toList
        Factors = assay.Tables |> Seq.map ArcTable.getAllFactors |> Seq.concat |> Seq.distinct |> Seq.toList
        AssociatedStudies = associatedStudiesForAssay investigation assay
        PreviousAssayIdentifiers = getPreviousAssayIdsForAssay assay investigation
        PreviousStudyIdentifiers = getPreviousStudyIdsForAssay assay investigation
        FollowingAssayIdentifiers = getFollowingAssayIdsForAssay assay investigation
        FollowingStudyIdentifiers = getFollowingStudyIdsForAssay assay investigation
        DataFileCount = Some (assay.Tables |> ResizeArray.map getDataFiles |> Seq.sum)
    }

    // let kwfjnfk (aO:AssayOverview) =
    //     printfn $"Previous Assays found: %A" AssayOverview.Previous

module Template =    // template part definitions 



    //Part 1: Title, Description, Contacts and Publication of Investigation
    let createInvTitle (tlm:TopLevelMetadata) : string =
        let sb = StringBuilder()

        if tlm.Title.IsSome then 
            sb.AppendLine($"# {tlm.Title.Value}") |> ignore
        else sb.AppendLine("# Please add a valid title for your ArcInvestigation") |> ignore
        sb.ToString()

    let createInvDescription (tlm:TopLevelMetadata) : string =
        let sb = StringBuilder()
        if tlm.Description.IsSome then
            sb.AppendLine($"### Description\n{tlm.Description.Value}") |> ignore
        else sb.AppendLine("### Please add a valid description to your ArcInvestigation") |> ignore

        sb.ToString()

    let createContactsSection (tlm:TopLevelMetadata) : string = // Optional
        let sb = StringBuilder()

        if tlm.Contacts.IsEmpty = false then           
            sb.AppendLine("## Contacts \n ") |> ignore 
            sb.AppendLine("| Names | Email | Address | Affiliation | ORCID |") |> ignore 
            sb.AppendLine("| ----- | ----- | ------- | ----------- | ----- |") |> ignore 
            let persons = tlm.Contacts |> List.map (fun p -> 
                let firstName = toString p.FirstName
                let midInitials = toString p.MidInitials
                let lastName = toString p.LastName
                let eMail = toString p.EMail
                let address = toString p.Address
                let affiliation = toString p.Affiliation
                let orcid = toString p.ORCID
                $"| {firstName} {midInitials} {lastName} | {eMail} | {address} | {affiliation} | {orcid} |") 
            let grouped = String.Join(" \n ", persons)
            sb.AppendLine($"{grouped}") |> ignore 
        sb.ToString()

    let createPublicationsSection (tlm:TopLevelMetadata) : string = // Optional
        let sb = StringBuilder()

        if tlm.Publications.IsEmpty = false then 
            sb.AppendLine("## Publication \n") |> ignore
            sb.AppendLine("| Meta Data | Description |") |> ignore
            sb.AppendLine("| ----------- | ----------- |") |> ignore
            let pubTitle = 
                let getTitle = tlm.Publications |> List.map (fun p -> p.Title.Value)
                String.Join(" , ", getTitle)
            sb.AppendLine($"| Title | {pubTitle} |") |> ignore
            let authors = 
                let getAuthors = tlm.Publications |> List.map (fun p -> p.Authors.Value)
                String.Join(" , ", getAuthors)
            sb.AppendLine($"| Authors | {authors} |") |> ignore
            let pubMedID = 
                let getPMID = tlm.Publications |> List.map (fun p -> p.PubMedID.Value)
                String.Join(" , ", getPMID)
            sb.AppendLine($"| PubMedID | {pubMedID} |") |> ignore
            let doi = 
                let getDOI = tlm.Publications |> List.map (fun p -> p.DOI.Value)
                String.Join(" , ", getDOI)
            sb.AppendLine($"| DOI | {doi} |") |> ignore
            let status = 
                let getStat = tlm.Publications |> List.map (fun p -> p.Status.Value.NameText)
                String.Join(" , ", getStat)
            sb.AppendLine($"| Current status | {status} |") |> ignore
        sb.ToString()


    // Part 2: Relationship Graph & Overview Table
    let createRelationshipGraph (tlm:TopLevelMetadata) (investigation:ArcInvestigation) (assayOVs:seq<AssayOverview>) (studyOVs:seq<StudyOverview>) : string =
        let sb = StringBuilder()

        let relationshipGraph = getRelationshipGraph investigation assayOVs studyOVs
        sb.AppendLine($"## Relationships between Assays and Studies \n {relationshipGraph} ")|> ignore
        // sb.AppendLine("_This flowchart highlights the relationship between assays (highlighted in blue) and studies (highlighted in green)_") |> ignore
        sb.ToString()

    let createOverviewTable (tlm:TopLevelMetadata) : string =
        let sb = StringBuilder()
        
        sb.AppendLine("### Overview Table") |> ignore
        sb.AppendLine("| Meta Data | Description |") |> ignore
        sb.AppendLine("| --------- | ----------- |") |> ignore
        if tlm.SubmissionDate.IsSome then
            sb.AppendLine($"| Submission Date | {tlm.SubmissionDate.Value} |") |> ignore
        else sb.AppendLine("| Submission Date  | tba |") |> ignore
        if tlm.PublicReleaseDate.IsSome then
            sb.AppendLine($"| Public Release Date | {tlm.PublicReleaseDate.Value} |") |> ignore
        else sb.AppendLine("| Public Release Date | tba |") |> ignore
        let studyIDs = String.Join(" , ", tlm.StudyIdentifiers)
        sb.AppendLine($"| Study identifiers | {studyIDs} |") |> ignore
        sb.AppendLine($"| Study Count | {tlm.StudyCount.Value} |") |> ignore
        let assayIDs = String.Join(" , ", tlm.AssayIdentifiers)
        sb.AppendLine($"| Assay identifiers | {assayIDs} |") |> ignore
        sb.AppendLine($"| Assay Count | {tlm.AssayCount.Value} |") |> ignore

        sb.ToString()




   
    //Part 3: Studies: Description, Additional details, Annotation headers
    let createStudySectionId(sOV:StudyOverview) : string =
        let sb = StringBuilder()
        sb.AppendLine($"## Study: _{sOV.StudyIdentifier}_") |> ignore
        sb.ToString()
    
    let createStudyTitle (sOV:StudyOverview) : string =
        let sb = StringBuilder()
        if sOV.StudyTitle.IsSome then sb.AppendLine($"## {sOV.StudyTitle.Value}") |> ignore
        sb.ToString()
    
    let createStudyDescription (sOV:StudyOverview) : string =
        let sb = StringBuilder()
        if sOV.StudyDescription.IsSome then sb.AppendLine($"### Description\n{sOV.StudyDescription.Value}") |> ignore
        sb.ToString()

    let createStudyAdditionalDetails (sOV:StudyOverview) : string =
        let sb = StringBuilder()
        sb.AppendLine("| Meta Data | Description |") |> ignore
        sb.AppendLine("| --------- | ----------- |") |> ignore
        sb.AppendLine($"| Table Count | {sOV.TableCount.Value} |") |> ignore
        let tableNamesString = String.Join(" , ", sOV.TableNames)
        sb.AppendLine($"| Table Names | {tableNamesString} |") |> ignore
        sb.AppendLine($"| Sample Count | {sOV.SampleCount.Value} |") |> ignore
        sb.AppendLine($"| Data File Count | {sOV.DataFileCount.Value} |") |> ignore
        if not sOV.AssociatedAssays.IsEmpty then 
            let asssociatedAssays = join "," (sOV.AssociatedAssays |> List.toArray)
            sb.AppendLine($"| Associated assays | {asssociatedAssays} |") |> ignore
        if not sOV.Organisms.IsEmpty then
            let organism = String.Join(" , ", (sOV.Organisms |> List.map (fun oa -> oa.NameText)))
            sb.AppendLine($"| Organisms | _{organism}_ |") |> ignore
        if not sOV.Genotypes.IsEmpty then
            let genotype = String.Join(" , ", (sOV.Genotypes |> List.map (fun oa -> oa.NameText)))
            sb.AppendLine($"| Genotype | _{genotype}_ |") |> ignore
        sb.AppendLine($"| Biological replicates | {sOV.BiologicalReplicateCount} |") |> ignore
        if sOV.TimepointCount.Value > 0 = true then 
            let tps = String.Join(" , ", sOV.TimepointCount.Value)
            sb.AppendLine($"| Time points | {tps} |") |> ignore
        sb.ToString()


    let createStudyAnnotationHeaders (sOV:StudyOverview) : string =
        let sb = StringBuilder()
        sb.AppendLine("### Annotation headers") |> ignore
        if sOV.Characteristics.IsEmpty = false then 
            let charString = getHyperlinks sOV.Characteristics
            sb.AppendLine($"\n**Characteristics**: {charString}") |> ignore
        if sOV.Parameters.IsEmpty = false then 
            let paramsString = getHyperlinks sOV.Parameters
            sb.AppendLine($"\n**Parameters**: {paramsString}") |> ignore
        if sOV.Factors.IsEmpty = false then 
            let factString = getHyperlinks sOV.Factors    
            sb.AppendLine($"\n**Factors**: {factString}") |> ignore
        sb.ToString()

    // Part 4: Assays: Description, Additional details, Annotation headers
    let createAssaySectionId (aOV:AssayOverview) : string =
        let sb = StringBuilder()
        sb.AppendLine($"## Assay: _{aOV.AssayIdentifier}_") |> ignore
        sb.ToString()
    
    let createAssayTitle (aOV:AssayOverview) : string =
        let sb = StringBuilder()
        if aOV.AssayTitle.IsSome then sb.AppendLine($"## {aOV.AssayTitle.Value}") |> ignore
        sb.ToString()

    let createAssayDescription (aOV:AssayOverview) : string =
        let sb = StringBuilder()
        if aOV.AssayDescription.IsSome then sb.AppendLine($"### Description\n{aOV.AssayDescription.Value}") |> ignore
        sb.ToString()

    let createAssayAdditionalDetails (aOV:AssayOverview) : string =
        let sb = StringBuilder()
        sb.AppendLine("| Meta Data | Description |") |> ignore
        sb.AppendLine("| --------- | ----------- |") |> ignore
        if aOV.MeasurementType.IsSome = true then // change to = true
            sb.AppendLine($"| Measurement Type | {aOV.MeasurementType.Value.NameText} |") |> ignore 
        if not aOV.MeasurementDevice.IsEmpty then 
            sb.AppendLine($"| Measurement Device | {aOV.MeasurementDevice.Head.Name.Value} |") |> ignore 
        if aOV.TechnologyType.IsSome = true then
            sb.AppendLine($"| Technology Type | {aOV.TechnologyType.Value.NameText} |") |> ignore
        sb.AppendLine($"| Table Count | {aOV.TableCount.Value} |") |> ignore
        let tableNamesString = String.Join(" , ", aOV.TableNames)
        sb.AppendLine($"| Table Names | {tableNamesString} |") |> ignore
        sb.AppendLine($"| Sample Count | {aOV.SampleCount.Value} |") |> ignore
        sb.AppendLine($"| Data File Count | {aOV.DataFileCount.Value} |") |> ignore
        if not aOV.AssociatedStudies.IsEmpty then
            let associatedStudies= join "," (aOV.AssociatedStudies |> List.toArray)
            sb.AppendLine($"| Associated studies | {associatedStudies} |") |> ignore
        sb.ToString()
    let createAssayAnnotationHeaders (aOV:AssayOverview) : string =
        let sb = StringBuilder()
        sb.AppendLine("### Annotation headers") |> ignore
        if aOV.Characteristics.IsEmpty = false then 
            let charString = getHyperlinks aOV.Characteristics
            sb.AppendLine($"\n**Characteristics**: {charString}") |> ignore
        if aOV.Parameters.IsEmpty = false then 
            let paramsString = getHyperlinks aOV.Parameters
            sb.AppendLine($"\n**Parameters**: {paramsString}") |> ignore
        if aOV.Factors.IsEmpty = false then 
            let factString = getHyperlinks aOV.Factors    
            sb.AppendLine($"\n**Factors**: {factString}") |> ignore
        sb.ToString()


    type TableOfContents =


        static member createAnchor (prefix:string) (ids:seq<string>) =
                ids
                |> Seq.map (fun (id:string) ->
                    $"     - [{id}](#{prefix}-{id.ToLower()})")
                |> String.concat "\n"

        static member createTOC(sections : Section list, tlm : TopLevelMetadata , ?assayOVs : seq<AssayOverview>, ?studyOVs : seq<StudyOverview>) =    
            let mutable studiesHeaderSet = false              
            let mutable assayHeaderSet = false

            let studyOVs = Option.defaultValue Seq.empty studyOVs
            let assayOVs = Option.defaultValue Seq.empty assayOVs

            let sb = StringBuilder()
            sb.AppendLine("## Table of Contents \n") |> ignore

            for sec in sections do
                match sec with 
                | Section.ISAGraph -> // no check if Empty 
                    sb.AppendLine("- [ISA Graph](#relationships-between-assays-and-studies)") |> ignore
                | Section.OverviewTable -> // no check if Empty 
                    sb.AppendLine("- [Overview Table](#overview-table)") |> ignore

                | Section.Investigation InvestigationSection.Contacts when not (tlm.Contacts.IsEmpty) ->
                    sb.AppendLine("- [Contacts](#contacts)") |> ignore
                | Section.Investigation InvestigationSection.Publication when not (tlm.Publications.IsEmpty) ->
                    sb.AppendLine("- [Publication](#publication)") |> ignore     
                | Section.Studies subSection when not (Seq.isEmpty studyOVs) ->
                    if not studiesHeaderSet then 
                        sb.AppendLine("- Studies \n ") |> ignore
                        studiesHeaderSet <- true
                        sb.AppendLine(TableOfContents.createAnchor "study" (studyOVs |> Seq.map (fun (sOV:StudyOverview) -> sOV.StudyIdentifier))) |> ignore

                | Section.Assays subSection when not (Seq.isEmpty assayOVs) -> 
                    if not assayHeaderSet then 
                        sb.AppendLine("- Assays \n ") |> ignore
                        assayHeaderSet <- true
                        sb.AppendLine(TableOfContents.createAnchor "assay" (assayOVs |> Seq.map (fun (aOV:AssayOverview) -> aOV.AssayIdentifier))) |> ignore            
                | Section.Investigation InvestigationSection.Title 
                | Section.Investigation InvestigationSection.Description
                | Section.TOC -> ()
                | _ -> ()
            sb.ToString()
