namespace ARCSummary


open System.IO
open YAMLicious
open YAMLicious.YAMLiciousTypes
open ConfigFileTypes
open SummaryStyles


module ConfigFileDecode =


    let yamlPath = Path.Combine(".arc","arc-summary.yml")
    let yamlContent = File.ReadAllText(yamlPath)
    let parsed = Reader.read yamlContent


    let decodeInvSection (element:YAMLElement) =
        match element with
        | YAMLElement.Object [YAMLElement.Value v] ->
            match v.Value.ToLower() with
            | "title" -> Some InvestigationSection.Title 
            | "description" -> Some InvestigationSection.Description 
            | "publication" -> Some Publication 
            | "contacts" -> Some Contacts 
            | _ -> None
        | _ -> None

    let decodeAssaySection (element:YAMLElement) =
        match element with
        | YAMLElement.Object [YAMLElement.Value v] ->
            match v.Value.ToLower() with
            | "title" -> Some AssaySection.Title
            | "description" -> Some AssaySection.Description
            | "additionaldetails" -> Some AssaySection.AdditionalDetails 
            | "annotationheaders" -> Some AssaySection.AnnotationHeaders 
            | _ -> None
        | _ -> None


    let decodeStudySection (element:YAMLElement) =
        match element with       
        | YAMLElement.Object [YAMLElement.Value v] ->
            match v.Value.ToLower() with
            | "title" -> Some StudySection.Title
            | "description" -> Some StudySection.Description
            | "additionaldetails" -> Some StudySection.AdditionalDetails 
            | "annotationheaders" -> Some StudySection.AnnotationHeaders 
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
        let theme = 
            match configYAML with 
            | YAMLElement.Object [YAMLElement.Value key]  -> 
                match key.Value.ToLower() with 
                | "publicationstyle" ->  PublicationStyle
                | _ -> Default
            | _ -> Default
        let custom = 
            match configYAML with
            | YAMLElement.Object elements ->
                elements 
                |> List.tryPick(function
                    | YAMLElement.Mapping (key, YAMLElement.Object [YAMLElement.Sequence secFields]) when key.Value.ToLower() = "custom" ->
                    Some (secFields |> List.collect decodeSection)
                    | _ -> None
                )
                |> Option.defaultValue defaultOrder
            | _ -> 
                if theme = Default then 
                    printfn "YAMLElement is not an object, instead is %A" configYAML
                defaultOrder
        {
            Theme = theme
            Custom = custom
        }

    let loadSectionOrder (arcPath: string) : Section list =
        let sectionsPath = Path.Combine(arcPath, ".arc", "arc-summary.yml")
        if not (File.Exists sectionsPath) then
            printfn "No arc-summary.yml found, using default section order."
            defaultOrder
        else
            let content = File.ReadAllText sectionsPath
            if System.String.IsNullOrWhiteSpace content then
                printfn "arc-summary.yml is empty, using default section order."
                defaultOrder
            else
                try
                    let config = decodeConfig (Reader.read content)
                    match config.Theme, config.Custom with
                    | PublicationStyle, _ ->
                        printfn "Using theme: PublicationStyle"
                        publicationStyle
                    | Default, custom when custom <> defaultOrder ->
                        printfn "Using custom section order from arc-summary.yml"
                        custom
                    | _, _ ->
                        printfn "No valid theme or custom order found, using default."
                        defaultOrder
                with _ ->
                    printfn "Failed to decode arc-summary.yml, using default section order."
                    defaultOrder













