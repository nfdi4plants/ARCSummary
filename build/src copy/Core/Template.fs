namespace ARCSummary


open ARCtrl
open ARCtrl.Helper
open System
open System.Text
open Option
open ArcQuerying
open Formating
open TemplateHelpers

module Instances =
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
        AssociatedAssays = associatedAssays investigation study
        PreviousAssayIdentifiers = getPreviousAssayIdentifiersforStudies study investigation
        PreviousStudyIdentifiers = getPreviousStudyIdentifiersforStudies study investigation
        FollowingAssayIdentifiers = getFollowingAssayIdentifiersforStudies study investigation
        FollowingStudyIdentifiers = getFollowingStudyIdentifiersforStudies study investigation
        DataFileCount = Some (study.Tables |> ResizeArray.map getDataFiles |> Seq.sum)
    } 

    let getAssayOverview (investigation:ArcInvestigation) (assay:ArcAssay) = {  
        AssayIdentifier = assay.Identifier
        AssayDescription = assay.Investigation.Value.Description
        MeasurementType = assay.MeasurementType
        MeasurementDevice = assay.Tables |> ResizeArray.collect getMeasurementDevice |> Seq.toList    
        TechnologyType = assay.TechnologyType                                                    
        TableCount = Some assay.TableCount                        
        TableNames = assay.TableNames
        Characteristics = assay.Tables |> Seq.map ArcTable.getAllCharacteristics |> Seq.concat |> Seq.distinct |> Seq.toList
        SampleCount = Some (assay.Tables |> getSampleCount)                                                                                                          
        Parameters =  assay.Tables |> Seq.map ArcTable.getAllParameters |> Seq.concat |> Seq.distinct |> Seq.toList
        Factors = assay.Tables |> Seq.map ArcTable.getAllFactors |> Seq.concat |> Seq.distinct |> Seq.toList
        AssociatedStudies = associatedStudies investigation assay
        PreviousAssayIdentifiers = getPreviousAssayIdentifiersforAssays assay investigation
        PreviousStudyIdentifiers = getPreviousStudyIdentifiersforAssays assay investigation
        FollowingAssayIdentifiers = getFollowingAssayIdentifiersforAssays assay investigation
        FollowingStudyIdentifiers = getFollowingStudyIdentifiersforAssays assay investigation
        DataFileCount = Some (assay.Tables |> ResizeArray.map getDataFiles |> Seq.sum)
    }



module Template =    // template part definitions 


    //Part 1: Title, Description, Mermaid Graph & TLM data 
    let createIntroSection (tlm:TopLevelMetadata) (investigation:ArcInvestigation) (assayOVs:seq<AssayOverview>) (studyOVs:seq<StudyOverview>) : string =
        let sb = StringBuilder()

        if tlm.Title.IsSome then 
            sb.AppendLine($"# {tlm.Title.Value}") |> ignore
        else sb.AppendLine("# Please add a valid title for your ArcInvestigation") |> ignore

        if tlm.Description.IsSome then
            sb.AppendLine($"### Description\n{tlm.Description.Value}") |> ignore
        else sb.AppendLine("### Please add a valid description to your ArcInvestigation") |> ignore

        let mermaidGraph = getMermaidGraph investigation assayOVs studyOVs
        sb.AppendLine($" ## Relationships between Assays and Studies \n {mermaidGraph} ")|> ignore
        sb.AppendLine("_Figure 1:This flowchart highlights the relationship between assays (highlighted in blue) and studies (highlighted in green)_") |> ignore
        
        sb.AppendLine("### Additional details") |> ignore
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

    // Part 2: Table of Contents, Publications & Contacts if given 

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

    //Part 3: Studies
    let createStudyMarkdownSection (sOV:StudyOverview) : string =
        let sb = StringBuilder()

        sb.AppendLine($"## Study : _{sOV.StudyIdentifier}_") |> ignore
        if sOV.StudyDescription.IsSome then
            sb.AppendLine($"### Description\n{sOV.StudyDescription.Value}") |> ignore
        else sb.AppendLine($"### Description\n tba ") |> ignore

        sb.AppendLine("### Additional details") |> ignore
        sb.AppendLine("| Meta Data | Description |") |> ignore
        sb.AppendLine("| --------- | ----------- |") |> ignore
        sb.AppendLine($"| Table Count | {sOV.TableCount.Value} |") |> ignore
        let tableNamesString = String.Join(" , ", sOV.TableNames)
        sb.AppendLine($"| Table Names | {tableNamesString} |") |> ignore
        sb.AppendLine($"| Sample Count | {sOV.SampleCount.Value} |") |> ignore
        sb.AppendLine($"| Data File Count | {sOV.DataFileCount.Value} |") |> ignore
        let laString = String.Join(" , ", sOV.AssociatedAssays)
        sb.AppendLine($"| Associated assays | {laString} |") |> ignore
        if sOV.Organisms.IsEmpty = false then
            let organism = String.Join(" , ", (sOV.Organisms |> List.map (fun oa -> oa.NameText)))
            sb.AppendLine($"| Organisms | _{organism}_ |") |> ignore
        if sOV.Genotypes.IsEmpty = false then
            let genotype = String.Join(" , ", (sOV.Genotypes |> List.map (fun oa -> oa.NameText)))
            sb.AppendLine($"| Genotype | _{genotype}_ |") |> ignore
        sb.AppendLine($"| Biological replicates | {sOV.BiologicalReplicateCount} |") |> ignore
        if sOV.TimepointCount.Value > 0 = true then 
            let tps = String.Join(" , ", sOV.TimepointCount.Value)
            sb.AppendLine($"| Time points | {tps} |") |> ignore

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

    // Part 4: Assays
    let createAssayMarkdownSection (aOV:AssayOverview) : string =
        let sb = StringBuilder()
        
        sb.AppendLine($"## Assay : _{aOV.AssayIdentifier}_") |> ignore
        if aOV.AssayDescription.IsSome then 
            sb.AppendLine($"### Description\n{aOV.AssayDescription.Value}") |> ignore
        else sb.AppendLine("### Description\n tba") |> ignore

        sb.AppendLine("### Additional details") |> ignore // changed to ignore since the value else is unused
        sb.AppendLine("| Meta Data | Description |") |> ignore
        sb.AppendLine("| --------- | ----------- |") |> ignore
        if aOV.MeasurementType.IsSome then
            sb.AppendLine($"| Measurement Type | {aOV.MeasurementType.Value.NameText} |") |> ignore  //let measurementType = maybe here not
        // else sb.AppendLine("") |> ignore 
        if aOV.MeasurementDevice.IsEmpty = false then 
            sb.AppendLine($"| Measurement Device | {aOV.MeasurementDevice.Head.Name.Value} |") |> ignore // check if oa to value.NameText is needed
        // else sb.AppendLine("") |> ignore
        if aOV.TechnologyType.IsSome then
            sb.AppendLine($"| Technology Type | {aOV.TechnologyType.Value.NameText} |") |> ignore
        // else sb.AppendLine("") |> ignore
        sb.AppendLine($"| Table Count | {aOV.TableCount.Value} |") |> ignore
        let tableNamesString = String.Join(" , ", aOV.TableNames)
        sb.AppendLine($"| Table Names | {tableNamesString} |") |> ignore
        sb.AppendLine($"| Sample Count | {aOV.SampleCount.Value} |") |> ignore
        sb.AppendLine($"| Data File Count | {aOV.DataFileCount.Value} |") |> ignore
        let lsString = String.Join(" , ", aOV.AssociatedStudies)
        sb.AppendLine($"| Associated studies | {lsString} |") |> ignore

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
