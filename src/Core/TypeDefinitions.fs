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
        AssayCount : int option ;
        StudyIdentifiers : string list ;
        StudyCount : int option
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

    type Theme =
        | Default
        | PublicationStyle // Title, Description, Contacts, Publication (check if available title with publication title)

    type ARCSummaryConfig = {
        Theme : Theme ;
        Custom : Section list
    }


    // themes as parameter that can be overwritten by custom

