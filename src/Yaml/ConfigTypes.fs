namespace ARCSummary


open ARCtrl.Yaml
open System.IO
open YAMLicious 
open YAMLicious.YAMLiciousTypes
open READMEAutomation


module ConfigFileTypes =

    type InvestigationSection =
        | Title
        | Description 
        | Publication
        | Contacts 
    type AssaySection =
        | Description
        | AdditionalDetails
        | AnnotationHeaders
    type StudySection =
        | Description 
        | AdditionalDetails
        | AnnotationHeaders

    type Section =
        | Investigation of InvestigationSection //option
        | TOC        //| Layout of LayoutSubSection list
        | ISAGraph
        | OverviewTable        
        | Assays of AssaySection 
        | Studies of StudySection 

    type ARCSummaryConfig = {
        Theme : unit option;
        Custom : Section list
    }

    type Theme =
        | PublicationStyle // Title, Description, Contacts, Publication (check if available title with publication title)

    // themes as parameter that can be overwritten by custom




