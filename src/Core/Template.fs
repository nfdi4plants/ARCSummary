namespace ARCSummary


open ARCtrl
open ARCtrl.Helper
open ARCtrl.WorkflowGraph
open System
open System.Text
open Option
open ArcQuerying
open Formating
open ProvenanceGraph
open SummaryTypes
open ConfigFileTypes
open ListHelper
open StringHelper
open Option

module ARCInstances =
    let getTopLevelMetadata (investigation:ArcInvestigation) : TopLevelMetadata = {
        Title = investigation.Title 
        Description = investigation.Description 
        SubmissionDate = investigation.SubmissionDate 
        PublicReleaseDate = investigation.PublicReleaseDate 
        Publications = investigation.Publications |> Seq.toList 
        Contacts = investigation.Contacts |> Seq.toList 
        AssayIdentifiers = investigation.AssayIdentifiers |> Seq.toList 
        AssayCount = Some investigation.AssayCount 
        StudyIdentifiers = investigation.StudyIdentifiers |> Seq.toList 
        StudyCount = Some investigation.StudyCount
        WorkflowIdentifiers = investigation.WorkflowIdentifiers |> Seq.toList
        WorkflowCount = Some investigation.WorkflowCount
        RunIdentifiers = investigation.RunIdentifiers |> Seq.toList
        RunCount = Some investigation.RunCount
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
        SampleCount = study.Tables |> getSampleCount |> Seq.toList
        Parameters = study.Tables |> Seq.map ArcTable.getAllParameters |> Seq.concat |> Seq.distinct |> Seq.toList
        Factors = study.Tables |> Seq.map ArcTable.getAllFactors |> Seq.concat |> Seq.distinct |> Seq.toList
        AssociatedAssays = associatedAssaysForStudy investigation study
        PreviousAssayIdentifiers = getPreviousAssayIdsForStudy study investigation
        PreviousStudyIdentifiers = getPreviousStudyIdsForStudy study investigation
        FollowingAssayIdentifiers = getFollowingAssayIdsForStudy study investigation
        FollowingStudyIdentifiers = getFollowingStudyIdsForStudy study investigation
        DataFileCount = study.Tables |> Seq.map getDataFiles |> Seq.toList
    } 

    let getAssayOverview (investigation:ArcInvestigation) (assay:ArcAssay) = {  
        AssayIdentifier = assay.Identifier
        AssayTitle = assay.Title
        AssayDescription = assay.Description 
        MeasurementType = assay.MeasurementType
        MeasurementDevice = assay.Tables |> ResizeArray.collect getMeasurementDevice |> Seq.toList    
        TechnologyType = assay.TechnologyType         
        TechnologyPlatform = assay.TechnologyPlatform                                           
        TableCount = Some assay.TableCount                        
        TableNames = assay.TableNames
        Characteristics = assay.Tables |> Seq.map ArcTable.getAllCharacteristics |> Seq.concat |> Seq.distinct |> Seq.toList
        SampleCount = assay.Tables |> getSampleCount |> Seq.toList                                                                                                          
        Parameters =  assay.Tables |> Seq.map ArcTable.getAllParameters |> Seq.concat |> Seq.distinct |> Seq.toList
        Factors = assay.Tables |> Seq.map ArcTable.getAllFactors |> Seq.concat |> Seq.distinct |> Seq.toList
        AssociatedStudies = associatedStudiesForAssay investigation assay
        PreviousAssayIdentifiers = getPreviousAssayIdsForAssay assay investigation
        PreviousStudyIdentifiers = getPreviousStudyIdsForAssay assay investigation
        FollowingAssayIdentifiers = getFollowingAssayIdsForAssay assay investigation
        FollowingStudyIdentifiers = getFollowingStudyIdsForAssay assay investigation
        DataFileCount = assay.Tables |> Seq.map getDataFiles |> Seq.toList
    }

    // let kwfjnfk (aO:AssayOverview) =
    //     printfn $"Previous Assays found: %A" AssayOverview.Previous

module Template =    // template part definitions 



    //Part 1: Title, Description, Contacts and Publication of Investigation
    let createInvTitle (tlm:TopLevelMetadata) : string =
        let sb = StringBuilder()

        if not tlm.Title.IsNone then 
            sb.AppendLine($"# {tlm.Title.Value} \n") |> ignore
        else sb.AppendLine("# Please add a valid title for your ArcInvestigation") |> ignore
        sb.ToString()

    let createInvDescription (tlm:TopLevelMetadata) : string =
        let sb = StringBuilder()
        if not tlm.Description.IsNone then
            sb.AppendLine($"> {tlm.Description.Value}") |> ignore
        else sb.AppendLine("> Please add a valid description to your ArcInvestigation") |> ignore

        sb.ToString()

    let createContactsSection (tlm:TopLevelMetadata) : string = 
        let sb = StringBuilder()

        if not tlm.Contacts.IsEmpty then           
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

    let createPublicationsSection (tlm:TopLevelMetadata) : string = 
        let sb = StringBuilder()

        if not tlm.Publications.IsEmpty then

            let pubTitle  = joinList (fun (p:Publication) -> p.Title) tlm.Publications
            let authors   = joinList (fun (p:Publication) -> p.Authors) tlm.Publications
            let doi       = joinList (fun (p:Publication) -> p.DOI) tlm.Publications
            let pubMedID  = joinList (fun (p:Publication) -> p.PubMedID |> Option.map string) tlm.Publications
            let status    = joinList (fun (p:Publication) -> p.Status |> Option.map (fun s -> s.NameText)) tlm.Publications

            sb.AppendLine("## Publication \n") |> ignore
            sb.AppendLine("| Meta Data | Description |") |> ignore
            sb.AppendLine("| ----------- | ----------- |") |> ignore

            appendLineIfNotEmpty sb "Title" pubTitle
            appendLineIfNotEmpty sb "Authors" authors
            appendLineIfNotEmpty sb "PubMedID" pubMedID
            appendLineIfNotEmpty sb "DOI" doi
            appendLineIfNotEmpty sb "Current status" status

        sb.ToString()


    // Part 2: Relationship Graph & Overview Table
    let createRelationshipGraph (tlm:TopLevelMetadata) (investigation:ArcInvestigation) (assayOVs:seq<AssayOverview>) (studyOVs:seq<StudyOverview>) : string =
        let sb = StringBuilder()

        let relationshipGraph = getRelationshipGraph investigation assayOVs studyOVs
        sb.AppendLine($"## Provenance graph Studies and Assays \n {relationshipGraph} ")|> ignore
        // sb.AppendLine("_This flowchart highlights the relationship between assays (highlighted in blue) and studies (highlighted in green)_") |> ignore
        sb.ToString()

    let createOverviewTable (tlm:TopLevelMetadata) : string =
        let sb = StringBuilder()
        sb.AppendLine("| Meta Data | Description |") |> ignore
        sb.AppendLine("| --------- | ----------- |") |> ignore
        if not tlm.SubmissionDate.IsNone then
            sb.AppendLine($"| Submission Date | {tlm.SubmissionDate.Value} |") |> ignore
        if not tlm.PublicReleaseDate.IsNone then
            sb.AppendLine($"| Public Release Date | {tlm.PublicReleaseDate.Value} |") |> ignore
        let studyIDs = String.Join(", ", tlm.StudyIdentifiers)
        sb.AppendLine($"| Study Identifiers | {studyIDs} |") |> ignore
        sb.AppendLine($"| Study Count | {tlm.StudyCount.Value} |") |> ignore
        let assayIDs = String.Join(", ", tlm.AssayIdentifiers)
        sb.AppendLine($"| Assay Identifiers | {assayIDs} |") |> ignore
        sb.AppendLine($"| Assay Count | {tlm.AssayCount.Value} |") |> ignore
        if not (tlm.WorkflowCount.Value = 0) then
            let workflowIDs = String.Join(", ", tlm.WorkflowIdentifiers)
            sb.AppendLine($"| Workflow Identifiers | {workflowIDs} |") |> ignore
            sb.AppendLine($"| Workflow Count | {tlm.WorkflowCount.Value} |") |> ignore
        if not (tlm.RunCount.Value = 0) then
            let runIDs = String.Join(", ", tlm.RunIdentifiers)
            sb.AppendLine($"| Run Identifiers | {runIDs} |") |> ignore
            sb.AppendLine($"| Run Count | {tlm.RunCount.Value} |") |> ignore
        sb.ToString()




   
    //Part 3: Studies: Description, Additional details, Annotation headers
    let createStudySectionId(sOV:StudyOverview) : string =
        let sb = StringBuilder()
        sb.AppendLine($"## Study: _{sOV.StudyIdentifier}_") |> ignore
        sb.ToString()
    
    let createStudyTitle (sOV:StudyOverview) : string =
        let sb = StringBuilder()
        if not sOV.StudyTitle.IsNone then sb.AppendLine($"## {sOV.StudyTitle.Value}") |> ignore
        sb.ToString()
    
    let createStudyDescription (sOV:StudyOverview) : string =
        let sb = StringBuilder()
        if not sOV.StudyDescription.IsNone then sb.AppendLine($" \n >{sOV.StudyDescription.Value}") |> ignore
        sb.ToString()

    let createStudyAdditionalDetails (sOV:StudyOverview) : string =
        let sb = StringBuilder()
        sb.AppendLine("| Meta Data | Description |") |> ignore
        sb.AppendLine("| --------- | ----------- |") |> ignore
        sb.AppendLine($"| Table Count | {sOV.TableCount.Value} |") |> ignore
        let tableNamesString = String.Join(", ", sOV.TableNames)
        sb.AppendLine($"| Table Names | {tableNamesString} |") |> ignore
        let sampleCountString = String.Join(", ", sOV.SampleCount)
        sb.AppendLine($"| Sample Count | {sampleCountString} |") |> ignore
        let dataFileCountString = String.Join(", " , sOV.DataFileCount)
        sb.AppendLine($"| Data File Count | {dataFileCountString} |") |> ignore
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
        if not sOV.Characteristics.IsEmpty then 
            let charString = getHyperlinks sOV.Characteristics
            sb.AppendLine($"\n**Characteristics**: {charString}") |> ignore
        if not sOV.Parameters.IsEmpty then 
            let paramsString = getHyperlinks sOV.Parameters
            sb.AppendLine($"\n**Parameters**: {paramsString}") |> ignore
        if not sOV.Factors.IsEmpty then 
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
        if not aOV.AssayTitle.IsNone then sb.AppendLine($"## {aOV.AssayTitle.Value}") |> ignore
        sb.ToString()

    let createAssayDescription (aOV:AssayOverview) : string =
        let sb = StringBuilder()
        if not aOV.AssayDescription.IsNone then sb.AppendLine($" \n >{aOV.AssayDescription.Value}") |> ignore
        sb.ToString()

    let createAssayAdditionalDetails (aOV:AssayOverview) : string =
        let sb = StringBuilder()
        sb.AppendLine("| Meta Data | Description |") |> ignore
        sb.AppendLine("| --------- | ----------- |") |> ignore
        if not aOV.MeasurementType.IsNone then 
            sb.AppendLine($"| Measurement Type | {aOV.MeasurementType.Value.NameText} |") |> ignore 
        if not aOV.MeasurementDevice.IsEmpty then 
            sb.AppendLine($"| Measurement Device | {aOV.MeasurementDevice.Head.Name.Value} |") |> ignore 
        if not aOV.TechnologyType.IsNone then
            sb.AppendLine($"| Technology Type | {aOV.TechnologyType.Value.NameText} |") |> ignore
        if not aOV.TechnologyPlatform.IsNone then 
            sb.AppendLine($"| Technology Platform | {aOV.TechnologyPlatform.Value.NameText} |") |> ignore
        sb.AppendLine($"| Table Count | {aOV.TableCount.Value} |") |> ignore
        let tableNamesString = String.Join(", ", aOV.TableNames)
        sb.AppendLine($"| Table Names | {tableNamesString} |") |> ignore
        let sampleCountString = String.Join(", ", aOV.SampleCount)
        sb.AppendLine($"| Sample Count | {sampleCountString} |") |> ignore
        let dataFileCountString = String.Join(", ", aOV.DataFileCount)
        sb.AppendLine($"| Data File Count | {dataFileCountString} |") |> ignore
        if not aOV.AssociatedStudies.IsEmpty then
            let associatedStudies= join "," (aOV.AssociatedStudies |> List.toArray)
            sb.AppendLine($"| Associated studies | {associatedStudies} |") |> ignore
        sb.ToString()
    let createAssayAnnotationHeaders (aOV:AssayOverview) : string =
        let sb = StringBuilder()
        sb.AppendLine("### Annotation headers") |> ignore
        if not aOV.Characteristics.IsEmpty then 
            let charString = getHyperlinks aOV.Characteristics
            sb.AppendLine($"\n**Characteristics**: {charString}") |> ignore
        if not aOV.Parameters.IsEmpty then 
            let paramsString = getHyperlinks aOV.Parameters
            sb.AppendLine($"\n**Parameters**: {paramsString}") |> ignore
        if not aOV.Factors.IsEmpty then 
            let factString = getHyperlinks aOV.Factors    
            sb.AppendLine($"\n**Factors**: {factString}") |> ignore
        sb.ToString()

    //part 5: Workflows with WorkflowGraphs
    let getWorkflowOverview (workflow: ArcWorkflow) = {
        Identifier = workflow.Identifier
        Title = workflow.Title
        Description = workflow.Description
        Workflowtype = workflow.WorkflowType
        URI = workflow.URI
        Version = workflow.Version
        SubWorkflowIDs = workflow.SubWorkflowIdentifiers
    }
    let getWorkflowGraphMap (investigation: ArcInvestigation): Map<string, WorkflowGraph> =
        investigation.WorkflowIdentifiers
        |> Array.choose (fun id ->
            ArcInvestigation.tryGetWorkflow id investigation
            |> Option.bind (fun wf ->
                match Adapters.ofWorkflow wf with
                | Ok g -> Some (id, g)
                | Error _ -> None
            )
        )
        |> Map.ofArray

    let renderWorkflowGraph (graphMap: Map<string, WorkflowGraph>) (workflowId: string) =
        match Map.tryFind workflowId graphMap with
        | Some graph ->
            let graphMd = WorkflowGraphSiren.toMarkdown graph
            $"### Workflowgraph\n{graphMd}"
        | None ->
            "_No WorkflowGraph available_\n"
    let createWorkflowSection (investigation: ArcInvestigation) (wOV: WorkflowOverview) =
        let sb = StringBuilder()
        if investigation.WorkflowCount <> 0 then 
            if wOV.Title.IsSome then 
                sb.AppendLine($"### {wOV.Title.Value} \n") |> ignore
            if wOV.Description.IsSome then 
                sb.AppendLine($"> {wOV.Description.Value} \n") |> ignore
            let rows =
                [
                    wOV.Workflowtype
                    |> Option.map (fun oa -> $"| Workflow Type | {oa.NameText} |")
                    wOV.URI
                    |> Option.map (fun uri -> $"| URI | {uri} |")
                    wOV.Version
                    |> Option.map (fun v -> $"| Version | {v} |")
                    if wOV.SubWorkflowIDs.Count <> 0 then
                        let subWFIDs = wOV.SubWorkflowIDs |> Seq.toArray |> join ", "
                        Some $"| SubWorkflowIDs | {subWFIDs} |"
                    else None
                ]
                |> List.choose id
            if not rows.IsEmpty then
                sb.AppendLine("### Additional details") |> ignore   
                sb.AppendLine("| Meta Data | Description |") |> ignore
                sb.AppendLine("| --------- | ----------- |") |> ignore

                rows
                |> List.iter (fun row -> sb.AppendLine(row) |> ignore)
        sb.ToString()

    // part 6 Runs
    let getRunOverview (run: ArcRun) = {
        Identifier = run.Identifier
        Title = run.Title
        Description = run.Description
        MeasurementType = run.MeasurementType
        TechnologyPlatform = run.TechnologyPlatform
        TechnologyType = run.TechnologyType
        TableCount = run.TableCount
        TableNames = run.TableNames
        AdjacentWorkflows = run.WorkflowIdentifiers |> Seq.toArray
    }    

    let createRunSection (investigation:ArcInvestigation) (rOV:RunOverview) = 
        let sb = StringBuilder()
        if investigation.RunCount <> 0 then 
            if rOV.Title.IsSome then 
                sb.AppendLine($"### {rOV.Title.Value}") |> ignore
            if rOV.Description.IsSome then 
                sb.AppendLine($"> {rOV.Description.Value} \n") |> ignore
            let rows =
                [
                    rOV.MeasurementType
                    |> Option.map (fun oa -> $"| Measurement Type | {oa.NameText} |")
                    rOV.TechnologyPlatform
                    |> Option.map (fun oa -> $"| Technology Platform | {oa.NameText} |")
                    rOV.TechnologyType
                    |> Option.map (fun oa -> $"| Technology Type | {oa.NameText} |")
                    if rOV.TableCount <> 0 then 
                        Some $"| Table Count | {rOV.TableCount} |"
                    else None
                    if not rOV.TableNames.IsEmpty then 
                        let tableIDs = rOV.TableNames |> Seq.toArray |> join ", " 
                        Some $"| Table Names | {tableIDs} |"
                    else None
                    if rOV.AdjacentWorkflows.Length <> 0 then
                        let adWFs = rOV.AdjacentWorkflows |> join ", "
                        Some $"| Adjacent Workflows | {adWFs} |"
                ]
                |> List.choose id
            if not rows.IsEmpty then
                sb.AppendLine("### Additional details") |> ignore   
                sb.AppendLine("| Meta Data | Description |") |> ignore
                sb.AppendLine("| --------- | ----------- |") |> ignore
                rows
                |> List.iter (fun row -> sb.AppendLine(row) |> ignore)
        sb.ToString()

    type TableOfContents =


        static member createAnchor (prefix:string) (ids:seq<string>) =
                ids
                |> Seq.map (fun (id:string) ->
                    $"     - [{id}](#{prefix}-{id.ToLower()})")
                |> String.concat "\n"

        static member createTOC(sections : Section list, tlm : TopLevelMetadata , ?assayOVs : seq<AssayOverview>, ?studyOVs : seq<StudyOverview>, ?workflowOVs : seq<WorkflowOverview>, ?runOVs : seq<RunOverview>) =    
            let mutable studiesHeaderSet = false              
            let mutable assayHeaderSet = false
            let mutable workflowHeaderSet = false
            let mutable runHeaderSet = false

            let studyOVs = Option.defaultValue Seq.empty studyOVs
            let assayOVs = Option.defaultValue Seq.empty assayOVs
            let workflowOVs = Option.defaultValue Seq.empty workflowOVs
            let runOVs = Option.defaultValue Seq.empty runOVs

            let sb = StringBuilder()
            sb.AppendLine("## Table of Contents \n") |> ignore

            for sec in sections do
                match sec with 
                | Section.ProvenanceGraph ProvenanceGraphSection.AsISA -> // no check if Empty 
                    sb.AppendLine("- [Provenance Graph](#provenance-graph-studies-and-assays)") |> ignore
                | Section.ProvenanceGraph ProvenanceGraphSection.AsArcTables ->
                    sb.AppendLine("- [Provenance Graph](#provenance-graph-of-arctables)") |> ignore
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
                | Section.Workflows subSection when not (Seq.isEmpty workflowOVs) -> 
                    if not workflowHeaderSet then 
                        sb.AppendLine("- Workflows \n ") |> ignore
                        workflowHeaderSet <- true
                        sb.AppendLine(TableOfContents.createAnchor "workflow" (workflowOVs |> Seq.map (fun (wOV:WorkflowOverview) -> wOV.Identifier))) |> ignore       
                | Section.Runs subSection when not (Seq.isEmpty runOVs) -> 
                    if not runHeaderSet then 
                        sb.AppendLine("- Runs \n ") |> ignore 
                        runHeaderSet <- true
                        sb.AppendLine(TableOfContents.createAnchor "run" (runOVs |> Seq.map (fun (rOV:RunOverview) -> rOV.Identifier))) |> ignore       
                | Section.Investigation InvestigationSection.Title 
                | Section.Investigation InvestigationSection.Description
                | Section.TOC -> ()
                | _ -> ()
            sb.ToString()
