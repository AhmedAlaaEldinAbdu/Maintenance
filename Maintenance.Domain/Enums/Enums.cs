﻿namespace Maintenance.Domain.Enums
{
    public enum State
    {
        NotDeleted,
        Deleted
    }
    public enum Table
    {
        JobLevel,
        JobDegree,
        Administrativelevel,
        SkeletonCategory,
        QualificationDepartment,
        Bank
    }

    public enum TableColumn
    {
        JobLevelId =1,
        JobDegreeId =2,
        AdministrativelevelId =3,
        SkeletonCategoryId =4,
        QualificationDepartmentId = 5,
        BankId = 6,
    }
    public enum UserType
    {
        Client=1,
        Technician,
        Owner ,
        Consultant
    }
   
    public enum ContractCategory
    {
        FestivalManagement,
        CamelClub,
    }

    public enum UsedCarCategory
    {
        Young,
        Big,
        Van,
    }
    public enum MonitoringStatus
    {
        Monitored ,
        UnMonitored
    }
    public enum Language
    {
        Arabic,
        English,
        German,
        Frensh,
        All
    }
    public enum Level
    {
        Strong,
        Medium,
        Weak
    } 
    public enum ComplanitTypeEnum
    {
        NotSecret,
        Secret
    }
    public enum BeneficiaryType
    {
        Internal,
        External,
        Group
    }
    public enum BeneficiaryInOrderType
    {
        All,
        BeneficiaryName,
        BeneficiaryType,
        SpecialName,
        State,
        Date,
        NumberOfItems
    }
    public enum StatusType
    {
        Active,
        DisActive
    }
    public enum OrderType
    {
        Ascending,
        Descending
    }
    public enum LangType
    {
        Ar,
        En
    }
    public enum FavType
    {
        Complanit,
        DataApi
    }
    public enum DataApiOwnerType
    {
        Owner,
        Beneficiary,
        None
    }
    public enum ComplanitUserType
    {
        Owner,
        HasAccess,
        CanRequestAccess
    }
    public enum RequestStage
    {
        ApplicationData,
        Candidacy,
        PersonalInterview,
        Test,
        Approval,
        Contract,
        Appointment,
    }
    public enum AgeGroup
    {
        From25To30,
    }
    public enum JobType
    {
        Temporary,
        Permanent
    }
    public enum Polarity
    {
        Negative,
        Postive,
    }
    public enum MeasuringIndicator
    {
        Percent,
        Dots,
    }
    public enum EvaluationPeriod
    {
        Weekly,
        Monthly,
        Quarterly,
        Yearly
    }
    public enum MaritalStateEnum
    {
        Married,
        Single
    }
    public enum NationalityEnum
    {
        SaudiArabia,
        Other
    }
    public enum EducationalQualificationEnum
    {
        دكتوراه,
        ماجستير,
        بكالوريوس,
        دبلوم,
        ثانوي,
        اخري
    }
    public enum UserGenderEnum
    {
        Male = 1,
        Female = 2,

    }
    public enum AttachmentType
    {
        ProfilePicture,
        AddressPicture,
        PassportPiture,
        IbanPicture,
        CvPicture,
        ContractPicture,

    }
    public enum RateKind
    {
        Club,
        Festival
    }
    public enum RateType
    {
        Weekly,
        Monthly,
        Quarterly,
        Yearly
    }
    public enum PermissionAuthorize
    {
        Others = -1,
        Employee,
        Department,
        All

    }
    public enum NotificationType
    {
        Message,
        RequestComplanit, 
    }
    public enum NotificationState
    {
        New,
        Approved,
        Rejected
    }
    
    public enum ComplanitStatus 
    {
        Submitted = 1,//create  مفتوح

        TechnicianAssigned = 2,// جاري المعالجة

        TechnicianSuspended=3,//معلق

        //ConsultantApprovalAfterSuspended = 4,

        TechnicianCanceled = 5,//ملغي

        //ConsultantApprovalAfterCanceled =6,

        TechnicianDone = 7,//مكتمل

        //ConsultantApprovalAfterDone = 8,
    
      //  TechnicianClosed = 9,//مقفول

    }
}