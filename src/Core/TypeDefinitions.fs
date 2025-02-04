namespace ARCSummary

open ARCtrl

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
    StudyDescription : string option ;
    TableCount : int option ;
    TableNames : string list ; 
    Characteristics : OntologyAnnotation list ; 
    Organisms : OntologyAnnotation list ;
    Genotypes : OntologyAnnotation list ;
    BiologicalReplicateCount : int ; 
    TimepointCount : int option ; 
    SampleCount : int option ;
    Parameters : OntologyAnnotation list ;
    Factors : OntologyAnnotation list ;
    AssociatedAssays : string list ; 
    PreviousAssayIdentifiers : string list ;
    PreviousStudyIdentifiers : string list ;
    FollowingAssayIdentifiers : string list ;
    FollowingStudyIdentifiers : string list ;
    DataFileCount : int option
} 

type AssayOverview = {
    AssayIdentifier : string ;
    AssayDescription : string option ;
    MeasurementType : OntologyAnnotation option ; 
    MeasurementDevice : OntologyAnnotation list  ;    
    TechnologyType : OntologyAnnotation option ;
    TableCount: int option ;
    TableNames: string list ; 
    Characteristics : OntologyAnnotation list ;   
    SampleCount : int option ;
    Parameters : OntologyAnnotation list ; 
    Factors : OntologyAnnotation list ;
    AssociatedStudies : string list  ; 
    PreviousAssayIdentifiers : string list ;
    PreviousStudyIdentifiers : string list ;
    FollowingAssayIdentifiers : string list ;
    FollowingStudyIdentifiers : string list ;
    DataFileCount : int option
}


