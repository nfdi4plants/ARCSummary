//namespace ARCSummary



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
open ARCtrl
open System
open System.IO
open TemplateHelpers
open ARCInstances
open Template

let yamlContentEx :string =
    File.ReadAllText("@/.arc/arc-summary.yml") //

let parsedEx = Reader.read yamlContentEx


    let createMarkdownOverview (sections : Section list) (investigation:ArcInvestigation) : string = 
        let tlm =
            getTopLevelMetadata investigation
        let assayOVs =
            investigation.Assays
            |> Seq.filter (fun (assay:ArcAssay) -> assay.TableCount <> 0)
            |> Seq.map (fun (assay:ArcAssay) -> getAssayOverview investigation assay)
        let studyOVs =
            investigation.Studies
            |> Seq.filter (fun (study:ArcStudy) -> study.TableCount <> 0)
            |> Seq.map (fun (study:ArcStudy) -> getStudyOverview investigation study)


        let intro =
            createIntroSection tlm 
        let relGraph =
            createRelationshipGraph tlm investigation assayOVs studyOVs
        let toC =
            createTableOfContents tlm assayOVs studyOVs
        let contacts = 
            createContactsSection tlm
        let publications = 
            createPublicationsSection tlm
        let studyOV =
            let studyString =
                studyOVs
                |> Seq.map createStudyMarkdownSection 
                |> Seq.toList
            String.Join("\n", studyString)
        let assayOV =
            let assayString =
                assayOVs
                |> Seq.map createAssayMarkdownSection 
                |> Seq.toList
            String.Join("\n", assayString) 
            
        sections
        |> Array.map (fun section ->
            match section with
            | Assay assaySubSections -> assayFunc assayOV
                assayOVs
                |> Seq.map createAssayMarkdownSection 
                |> Seq.toList
                String.Join("\n", assayString)
            | TOC -> tocFunc
    
        )
        |> String.join "\n"
        // String.Join("\n", [intro;toC;relGraph;contacts;publications;studyOV;assayOV])


    







