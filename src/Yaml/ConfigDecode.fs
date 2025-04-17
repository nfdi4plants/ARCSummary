namespace ARCSummary


open System.IO
open YAMLicious
open YAMLicious.YAMLiciousTypes
open ConfigFileTypes


module ConfigFileDecode =


    let yamlPath = Path.Combine(".arc","arc-summary.yml")
    let yamlContent = File.ReadAllText(yamlPath)
    let parsed = Reader.read yamlContent


    let decodeInvestigationSubSection (parsedYML:YAMLElement) =
        match parsedYML with 
            | YAMLElement.Object [YAMLElement.Sequence v] ->
                v
                |> List.choose (function 
                    | YAMLElement.Object [YAMLElement.Value key ; YAMLElement.Sequence subsections]
                        when key.Value.ToLower() = "investigation" ->
                            Some (InvestigationSubSection.fromYamlElement (YAMLElement.Sequence subsections))
                    | _ -> None
                )
                |> List.concat
            | _ -> []

    let decodeAssaySubSection (parsedYML:YAMLElement) =
        match parsedYML with 
            | YAMLElement.Object [YAMLElement.Sequence v] ->
                v
                |> List.choose (function 
                    | YAMLElement.Object [YAMLElement.Value key ; YAMLElement.Sequence subsections]
                        when key.Value.ToLower() = "assays" ->
                            Some (AssaySubSection.fromYamlElement (YAMLElement.Sequence subsections))
                    | _ -> None
                )
                |> List.concat
            | _ -> []


    let decodeStudySubSection (parsedYML:YAMLElement) =
        match parsedYML with 
            | YAMLElement.Object [YAMLElement.Sequence v] ->
                v
                |> List.choose (function 
                    | YAMLElement.Object [YAMLElement.Value key ; YAMLElement.Sequence subsections]
                        when key.Value.ToLower() = "studies" ->
                            Some (StudySubSection.fromYamlElement (YAMLElement.Sequence subsections))
                    | _ -> None
                )
                |> List.concat
            | _ -> []


    let templateTypeDecoder : (YAMLElement -> Theme) = // additonal case-insensitivity handling with .Trim().ToLower or optional instead of required
            Decode.object (fun (get:Decode.IGetters) -> 
                match get.Optional.Field "Theme" Decode.string with 
                | Some "publicationstyle" -> PublicationStyle
                | _ -> failwith "No valid theme selected"
            )













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