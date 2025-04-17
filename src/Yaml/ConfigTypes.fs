namespace ARCSummary


open ARCtrl.Yaml
open System.IO
open YAMLicious 
open YAMLicious.YAMLiciousTypes
open READMEAutomation


module ConfigFileTypes =


    type InvestigationSubSection =
        | Title
        | Description 
        | Publication
        | Contacts 

        static member FromString (s:string) =
            match s.ToLower() with
            | "title" -> Some Title
            | "description" -> Some Description 
            | "publication" -> Some Publication
            | "contacts" -> Some Contacts 
            | _ -> None
            
        static member fromYamlElement (value:YAMLElement) : InvestigationSubSection list =
            match value with
            | YAMLElement.Sequence elements -> 
                elements 
                |> List.choose (fun el ->
                    match el with 
                    | YAMLElement.Object [YAMLElement.Value v] -> InvestigationSubSection.FromString v.Value
                    | _ -> None
                )
            | _ -> []


    type AssaySubSection =
        | Description
        | AdditionalInfo
        | AnnotationHeaders

        static member fromString (s:string) =
            match s.Trim().ToLower() with
            | "description" -> Some Description 
            | "additionalinfo" -> Some AdditionalInfo
            | "annotationheaders" -> Some AnnotationHeaders 
            | _ -> None
            
        static member fromYamlElement (value:YAMLElement) : AssaySubSection list =
            match value with
            | YAMLElement.Sequence elements -> 
                elements 
                |> List.choose (fun el ->
                    match el with 
                    | YAMLElement.Value v -> AssaySubSection.fromString v.Value
                    | _ -> None
                )
            | _ -> []

    type StudySubSection =
        | Description 
        | AdditionalInfo
        | AnnotationHeaders

        static member fromString (s:string) =
            match s.Trim().ToLower() with
            | "description" -> Some Description 
            | "additionalinfo" -> Some AdditionalInfo
            | "annotationheaders" -> Some AnnotationHeaders 
            | _ -> None
            
        static member fromYamlElement (value:YAMLElement) : StudySubSection list =
            match value with
            | YAMLElement.Sequence elements -> 
                elements 
                |> List.choose (fun el ->
                    match el with 
                    | YAMLElement.Value v -> StudySubSection.fromString v.Value
                    | _ -> None
                )
            | _ -> []

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




