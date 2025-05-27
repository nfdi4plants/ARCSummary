namespace ARCSummary


open ConfigFileTypes


module SummaryStyles = //hardcoded section order for default case and themes
    let defaultOrder : Section list = 
        [
            Section.Investigation InvestigationSection.Title
            Section.Investigation InvestigationSection.Description
            Section.TOC
            Section.ISAGraph
            Section.OverviewTable
            Section.Investigation InvestigationSection.Contacts
            Section.Investigation InvestigationSection.Publication
            Section.Studies StudySection.Title
            Section.Studies StudySection.Description
            Section.Studies StudySection.AdditionalDetails
            Section.Studies StudySection.AnnotationHeaders       
            Section.Assays AssaySection.Title 
            Section.Assays AssaySection.Description
            Section.Assays AssaySection.AdditionalDetails
            Section.Assays AssaySection.AnnotationHeaders
        ]
    
    let publicationStyle : Section list = // Title, Description, Contacts, Publication 
        [
            Section.Investigation InvestigationSection.Title
            Section.Investigation InvestigationSection.Description
            Section.Investigation InvestigationSection.Contacts
            Section.Investigation InvestigationSection.Publication
        ]
