namespace ARCSummary

open ARCtrl

module SummaryTypes =
    type TopLevelMetadata = { 
        Title : string option ;
        Description : string option ;
        SubmissionDate : string option ;
        PublicReleaseDate : string option ;
        Publications : Publication list ;
        Contacts : Person list ;
        AssayIdentifiers : string list ;
        AssayCount : int option ; // maybe int and display 0 when not, but match is made easier
        StudyIdentifiers : string list ;
        StudyCount : int option ;
        WorkflowCount : int option;
        RunCount : int option
    }


    // Record type Overview for shared fields between study and assay might be beneficial
    type StudyOverview = {
        StudyIdentifier : string ; 
        StudyTitle : string option ;
        StudyDescription : string option ;
        TableCount : int option ;
        TableNames : string list ; 
        Characteristics : OntologyAnnotation list ; 
        Organisms : OntologyAnnotation list ;
        Genotypes : OntologyAnnotation list ;
        BiologicalReplicateCount : int ; 
        TimepointCount : int option ; 
        SampleCount : int list ;
        Parameters : OntologyAnnotation list ;
        Factors : OntologyAnnotation list ;
        AssociatedAssays : string list ; 
        PreviousAssayIdentifiers : string list ;
        PreviousStudyIdentifiers : string list ;
        FollowingAssayIdentifiers : string list ;
        FollowingStudyIdentifiers : string list ;
        DataFileCount : int list
    } 

    type AssayOverview = {
        AssayIdentifier : string ;
        AssayTitle : string option ;
        AssayDescription : string option ;
        MeasurementType : OntologyAnnotation option ; 
        MeasurementDevice : OntologyAnnotation list  ;    
        TechnologyType : OntologyAnnotation option ;
        TechnologyPlatform : OntologyAnnotation option ;
        TableCount: int option ;
        TableNames: string list ; 
        Characteristics : OntologyAnnotation list ;   
        SampleCount : int list ;
        Parameters : OntologyAnnotation list ; 
        Factors : OntologyAnnotation list ;
        AssociatedStudies : string list  ; 
        PreviousAssayIdentifiers : string list ;
        PreviousStudyIdentifiers : string list ;
        FollowingAssayIdentifiers : string list ;
        FollowingStudyIdentifiers : string list ;
        DataFileCount : int list
    }

    type WorkflowOverview =
        {
            Identifier : string
            Title : string option
            Description : string option
            Workflowtype : OntologyAnnotation option  
            URI : string option
            Version : string option  
            SubWorkflowIDs : string ResizeArray
            // Parameters, Compontents sepperatly like in Assays and Studies maybe Datamap eventually, Contacts maybe to the Contacts section

            //WorkflowGraph : string 
        }


    type RunOverview = 
        {
            Identfier : string
            Title : string option
            Description : string option
            MeasurementType : OntologyAnnotation option
            TechnologyPlatform : OntologyAnnotation option
            TechnologyType : OntologyAnnotation option
            TableCount : int
            TableNames : string array
            AdjacentWorkflows : string array


            // Cwl description and input?,Performers? datamap..
        }


module ConfigFileTypes =

    type InvestigationSection =
        | Title
        | Description 
        | Publication
        | Contacts 
        
    type AssaySection =
        | Title 
        | Description
        | AdditionalDetails
        | AnnotationHeaders
    type StudySection =
        | Title 
        | Description
        | AdditionalDetails
        | AnnotationHeaders

    type WorkflowSection = // metadata could be split as with other sections
        | Metadata
        | WorkflowGraph
    type ProvenanceGraphSection = 
        | AsISA
        | AsArcTables
    type Section =
        | Investigation of InvestigationSection 
        //| Methods // not yet implemented AI-Assisted Summary based on prompt /src/LLM/Prompt.fs
        | TOC        
        | ProvenanceGraph of ProvenanceGraphSection
        | OverviewTable        
        | Assays of AssaySection 
        | Studies of StudySection 
        | Workflows of WorkflowSection

    type Theme =
        | Default
        | PublicationStyle // Title, Description, Contacts, Publication (check if available title with publication title)

    type ARCSummaryConfig = {
        Theme : Theme ;
        Custom : Section list
    }


    // themes as parameter that can be overwritten by custom

