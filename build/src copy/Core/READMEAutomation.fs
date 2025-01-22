namespace ARCSummary

open ARCtrl
open System
open System.IO
open TemplateHelpers
open Instances
open Template


module READMEAutomation = // better name rttrfh


    let createMarkdownOverview (investigation:ArcInvestigation) : string = //createMarkdownOverview
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
            createIntroSection tlm investigation assayOVs studyOVs  
        let toC =
            getTableOfContents tlm assayOVs studyOVs
        let contacts = // Names to similar here
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
            
        let refs = "## References \n _add your references here_"
        String.Join("\n", [intro;toC;contacts;publications;studyOV;assayOV;refs])

    /// function that handles creation of appended markdown, checks if content is already or in need of an update
    let updateMarkdownContent (preexistingMD:string) (investigation:ArcInvestigation) : string = 
        let automatedMD  = createMarkdownOverview investigation
        let startMarker = "<!--- Start of automated section -->"
        let endMarker = "<!--- End of automated section -->"
        if preexistingMD.Contains(startMarker) && preexistingMD.Contains(endMarker) then 
            let startIndex = preexistingMD.IndexOf(startMarker) + startMarker.Length
            let endIndex = preexistingMD.IndexOf(endMarker)
            let currentAutomatedContent = 
                preexistingMD.Substring(startIndex, endIndex - startIndex).Trim()
            if currentAutomatedContent = automatedMD then
                preexistingMD
            else
                let updatedContent =
                    preexistingMD.Substring(0, startIndex) +
                    $"\n{automatedMD}\n" +
                    preexistingMD.Substring(endIndex)
                updatedContent
        else String.Join("\n\n", [preexistingMD;startMarker;automatedMD;endMarker])
    
    let updateMarkdownFile (mdFileName: string) (investigation: ArcInvestigation) = 
        if File.Exists(mdFileName) then
            let existingContent = File.ReadAllText(mdFileName)
            let updatedMD = updateMarkdownContent existingContent investigation
            if existingContent <> updatedMD then
                File.WriteAllText(mdFileName, updatedMD)
            else
                printfn "File is already updated"
        else
            let automatedMD  = createMarkdownOverview investigation
            System.IO.FileInfo(mdFileName).Directory.Create() |> ignore
            File.WriteAllText(mdFileName, automatedMD)

    //Wrapper for correct path name (need to check arcPath for correct signature //)
    let updateREADME (arcPath:string) (investigation:ArcInvestigation) =
        let readmePath = System.IO.Path.Combine(arcPath, "README.md")
        updateMarkdownFile readmePath investigation  
        readmePath      

