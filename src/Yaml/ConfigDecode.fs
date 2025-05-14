namespace ARCSummary


open System.IO
open YAMLicious
open YAMLicious.YAMLiciousTypes
open ConfigFileTypes


module ConfigFileDecode =


    let yamlPath = Path.Combine(".arc","arc-summary.yml")
    let yamlContent = File.ReadAllText(yamlPath)
    let parsed = Reader.read yamlContent


    let decodeInvSection (element:YAMLElement) =
        match element with
        | YAMLElement.Object [YAMLElement.Value v] ->
            match v.Value.ToLower() with
            | "title" -> Some Title // (Sections.Investigation Title)
            | "description" -> Some InvestigationSection.Description //(Sections.Investigation InvestigationSection.Description)
            | "publication" -> Some Publication //(Sections.Investigation Publication)
            | "contacts" -> Some Contacts //(Sections.Investigation Contacts)
            | _ -> None
        | _ -> None

    let decodeAssaySection (element:YAMLElement) =
        match element with
        | YAMLElement.Object [YAMLElement.Value v] ->
            match v.Value.ToLower() with
            | "description" -> Some AssaySection.Description //(Sections.Assays AssaySection.Description)
            | "additionaldetails" -> Some AssaySection.AdditionalDetails //(Sections.Assays AssaySection.AdditionalDetails)
            | "annotationheaders" -> Some AssaySection.AnnotationHeaders //(Sections.Assays AssaySection.AnnotationHeaders)
            | _ -> None
        | _ -> None


    let decodeStudySection (element:YAMLElement) =
        match element with       
        | YAMLElement.Object [YAMLElement.Value v] ->
            match v.Value.ToLower() with
            | "description" -> Some StudySection.Description //(Sections.Studies StudySection.Description)
            | "additionaldetails" -> Some StudySection.AdditionalDetails //(Sections.Studies StudySection.AdditionalDetails)
            | "annotationheaders" -> Some StudySection.AnnotationHeaders //(Sections.Studies StudySection.AnnotationHeaders)
            | _ -> None
        | _ -> None

    let decodeSection (element:YAMLElement) : Section list =
        match element with
        | YAMLElement.Object [YAMLElement.Value key] ->
            match key.Value.ToLower() with
            | "toc" -> [TOC]
            | "isagraph" -> [ISAGraph]
            | "overviewtable" -> [OverviewTable]
            | key -> failwithf "Decode subsectionless section failed, unknown key %s" key
        | YAMLElement.Object [YAMLElement.Value key; YAMLElement.Sequence secFields] ->
            match key.Value.ToLower() with
            | "investigation" -> secFields |> List.choose (decodeInvSection >> Option.map Investigation)
            | "studies" -> secFields |> List.choose (decodeStudySection >> Option.map Studies)
            | "assays" -> secFields |> List.choose (decodeAssaySection >> Option.map Assays)          
            | key -> failwithf "Decode section failed, unknown key %s" key
        | _ -> failwithf "Decode section failed: Section must be object sequence tuple but is %A" element

    let decodeConfig (configYAML : YAMLElement) : ARCSummaryConfig =
        let theme = None
        let custom = 
            match configYAML with
            | YAMLElement.Object o ->
                match o with 
                | [YAMLElement.Mapping (key, YAMLElement.Object [YAMLElement.Sequence secFields])] ->
                    match key.Value.ToLower() with
                    | "custom" -> secFields |> List.collect decodeSection
                    | _ -> []
                | _ -> failwithf "YAML element is not a correct mapping, instead is %A" o
            | _ -> failwithf "YAML element is not an object, instead is %A" configYAML
        {
            Theme = theme
            Custom = custom
        }














    // let returnDefault =
    //     if yamlContent |> Seq.isEmpty = true then
    //         updateREADME  



        // Decode.fromYamlString

    // createYamlFile function



//     - TOC
//   - ISA Relationship Graph
//   - Overview Table

//   - Studies
//     - Description
//     - Additionaldetails
//     - Annotationheaders
//   - Assays
//     - Description
//     - Additionaldetails
//     - Annotationheaders