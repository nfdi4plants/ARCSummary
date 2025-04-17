//namespace ARCSummary


#r "nuget: YAMLicious, 0.0.3"

//
open System.IO
open YAMLicious 
open YAMLicious.YAMLiciousTypes
// open READMEAutomation
// open ConfigFileTypes




module ConfigFileDecode =


    type InvestigationSubSection =
        | Title
        | Description 
        | Publication
        | Contacts 

    type AssaySubSection =
        | Description
        | AdditionalInfo
        | AnnotationHeaders

    type StudySubSection =
        | Description 
        | AdditionalInfo
        | AnnotationHeaders

    type Section =
        | Investigation of InvestigationSubSection list
        | TOC
        | ISAGraph
        | OverviewTable
        | Assays of AssaySubSection list
        | Studies of StudySubSection list

    type Theme =
        | PublicationStyle // Title, Description, Contacts, Publication (check if available title with publication title)

    // themes as parameter that can be overwritten by custom


        static member templateTypeDecoder : (YAMLElement -> Theme) = // additonal case-insensitivity handling with .Trim().ToLowerInvariant() or optional instead of required
            Decode.object (fun (get:Decode.IGetters) -> 
                match get.Optional.Field "Theme" Decode.string with 
                | Some "publicationstyle" -> PublicationStyle
                | _ -> failwith "No valid theme selected"
            )

open ConfigFileDecode

#r "nuget: YAMLicious, 0.0.3"
#r "nuget: ARCtrl, 2.5.1"
#r "nuget: Fable.Core, 4.5.0"
//
open ARCtrl.Yaml
open System.IO
open YAMLicious 
open YAMLicious.YAMLiciousTypes
open System.IO
open YAMLicious.Decode.Helper    
open Fable.Core 

let yamlContent :string =
    File.ReadAllText("@/.arc/arc-summary.yml")




module ConfigTypes =


    type InvestigationSubSection =
        | Title
        | Description 
        | Publication
        | Contacts 

        static member ISSFromString (s:string) =
            match s.ToLower() with
            | "title" -> Some Title
            | "description" -> Some Description 
            | "publication" -> Some Publication
            | "contacts" -> Some Contacts 
            | _ -> None
            
        static member ISSFromYamlElement (value:YAMLElement) : InvestigationSubSection list =
            match value with
            | YAMLElement.Sequence elements -> 
                elements 
                |> List.choose (fun el ->
                    match el with 
                    | YAMLElement.Object [YAMLElement.Value v] -> InvestigationSubSection.ISSFromString v.Value
                    | _ -> None
                )
            | _ -> []

    type AssaySubSection =
        | Description
        | AdditionalInfo
        | AnnotationHeaders





        
// let parsed = Reader.read yamlContent


// let decodeInvestigationSubSection =
//     match parsed with 
//         | YAMLElement.Object [YAMLElement.Sequence v] ->
//             v
//             |> List.choose (function 
//                 | YAMLElement.Object [YAMLElement.Value key ; YAMLElement.Sequence subsections]
//                     when key.Value.ToLower() = "investigation" ->
//                         Some (InvestigationSubSection.fromYamlElement (YAMLElement.Sequence subsections))
//                 | _ -> None
//             )
//             |> List.concat
//         | _ -> []







        // member this.Sectionlist (sectionNames:string list) (value:YAMLElement) : InvestigationSubSection list =
        //     match value with 
        //     | YAMLElement.Value v ->
        //         match v.Value.ToLower() with
        //         | "title" -> Title
        //         | "description" -> Description
        //         | "publication" -> Publication
        //         | "contacts" -> Contacts 
        //         | _ -> failwith $"Unrecognized section {v.Value}" //YAMLicious.Decode.Helper 
        //     | _ -> []

// let getSections =
//     Decode.object (fun (get:Decode.IGetters) ->
//         get.Required.Field "sections" Decode.string)



// Decode.fromYamlString getSections yamlContent
// let decodeRes =
//     Decode.fromYamlString getSections yamlContent


// printfn "%s" decodeRes