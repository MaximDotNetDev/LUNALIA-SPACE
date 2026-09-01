USE [DB_SchoolJournal];
GO

SET NOCOUNT ON;
GO

IF SCHEMA_ID('Identity') IS NOT NULL
BEGIN
PRINT N'КРИТИЧНА ПОМИЛКА: Стара база не була видалена! Закрий інші вкладки, які тримають з''єднання з цією БД, зніми виділення тексту і запусти скрипт повністю (F5).';
SET NOEXEC ON;
END
GO

CREATE SCHEMA [Identity];
GO
CREATE SCHEMA [Core];
GO
CREATE SCHEMA [Operations];
GO
CREATE SCHEMA [Reference];
GO
CREATE SCHEMA [Infrastructure];
GO
CREATE SCHEMA [Communications];
GO

PRINT N'=== 1. Створення структури таблиць... ===';

CREATE TABLE [Reference].[Semesters] (
    SemesterId UNIQUEIDENTIFIER CONSTRAINT DF_Semesters_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Semesters PRIMARY KEY CLUSTERED,
    SemesterName NVARCHAR(50) NOT NULL CONSTRAINT CK_Semesters_SemesterName CHECK (LEN(TRIM(SemesterName)) > 0 AND SemesterName = LTRIM(RTRIM(SemesterName))),
    StartDate DATETIMEOFFSET NOT NULL,
    EndDate DATETIMEOFFSET NOT NULL,
    IsDeleted BIT CONSTRAINT DF_Semesters_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Semesters_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CK_Semesters_Dates CHECK (StartDate < EndDate)
) WITH (DATA_COMPRESSION = PAGE);

CREATE UNIQUE NONCLUSTERED INDEX IX_Semesters_SemesterName ON [Reference].[Semesters](SemesterName) WHERE IsDeleted = 0;

CREATE TABLE [Infrastructure].[SystemSettings] (
    SettingId UNIQUEIDENTIFIER CONSTRAINT DF_SystemSettings_Id DEFAULT NEWSEQUENTIALID() CONSTRAINT PK_SystemSettings PRIMARY KEY CLUSTERED,
    SettingKey INT CONSTRAINT DF_SystemSettings_SettingKey DEFAULT 1 NOT NULL CONSTRAINT CK_SystemSettings_SingleRow CHECK (SettingKey = 1),
    SchoolName NVARCHAR(200) NOT NULL CONSTRAINT CK_SystemSettings_SchoolName CHECK (LEN(TRIM(SchoolName)) > 0 AND SchoolName = LTRIM(RTRIM(SchoolName))),
    AcademicYear NVARCHAR(20) NOT NULL CONSTRAINT CK_SystemSettings_AcademicYear CHECK (LEN(TRIM(AcademicYear)) > 0 AND AcademicYear = LTRIM(RTRIM(AcademicYear))),
    PrincipalName NVARCHAR(100) NULL,
    UpdatedByUserId UNIQUEIDENTIFIER NOT NULL,
    IsDeleted BIT CONSTRAINT DF_SystemSettings_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_SystemSettings_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL
) WITH (DATA_COMPRESSION = PAGE);

CREATE UNIQUE NONCLUSTERED INDEX IX_SystemSettings_SingleRow ON [Infrastructure].[SystemSettings](SettingKey) WHERE IsDeleted = 0;

CREATE TABLE [Identity].[Roles] (
    RoleId UNIQUEIDENTIFIER CONSTRAINT DF_Roles_RoleId DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED,
    RoleName NVARCHAR(50) NOT NULL CONSTRAINT CK_Roles_RoleName CHECK (LEN(TRIM(RoleName)) > 0 AND RoleName = LTRIM(RTRIM(RoleName))),
    Description NVARCHAR(255) NULL,
    IsDeleted BIT CONSTRAINT DF_Roles_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Roles_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL
) WITH (DATA_COMPRESSION = PAGE);

CREATE UNIQUE NONCLUSTERED INDEX IX_Roles_RoleName ON [Identity].[Roles](RoleName) WHERE IsDeleted = 0;

CREATE TABLE [Identity].[Users] (
    UserId UNIQUEIDENTIFIER CONSTRAINT DF_Users_UserId DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Users PRIMARY KEY CLUSTERED,
    Login NVARCHAR(50) NOT NULL CONSTRAINT CK_Users_Login CHECK (LEN(TRIM(Login)) > 0 AND Login = LTRIM(RTRIM(Login))),
    Email NVARCHAR(100) NULL CONSTRAINT CK_Users_Email CHECK (Email LIKE '%@%'),
    PasswordHash NVARCHAR(255) NOT NULL CONSTRAINT CK_Users_PasswordHash CHECK (LEN(TRIM(PasswordHash)) > 0 AND PasswordHash = LTRIM(RTRIM(PasswordHash))),
    RoleId UNIQUEIDENTIFIER NOT NULL,
    LastLoginUtc DATETIMEOFFSET NULL,
    FailedLoginAttempts INT CONSTRAINT DF_Users_FailedLoginAttempts DEFAULT 0 NOT NULL CONSTRAINT CK_Users_FailedLoginAttempts CHECK (FailedLoginAttempts >= 0),
    LockoutEndUtc DATETIMEOFFSET NULL,
    IsActive BIT CONSTRAINT DF_Users_IsActive DEFAULT 1 NOT NULL,
    IsDeleted BIT CONSTRAINT DF_Users_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Users_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES [Identity].[Roles](RoleId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Users_CreatedAt ON [Identity].[Users](CreatedAt) WITH (DATA_COMPRESSION = PAGE);
CREATE UNIQUE NONCLUSTERED INDEX IX_Users_Login ON [Identity].[Users](Login) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Users_IsDeleted ON [Identity].[Users](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE UNIQUE NONCLUSTERED INDEX IX_Users_Email ON [Identity].[Users](Email) WHERE Email IS NOT NULL AND IsDeleted = 0;

CREATE TABLE [Reference].[Positions] (
    PositionId UNIQUEIDENTIFIER CONSTRAINT DF_Positions_PositionId DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Positions PRIMARY KEY CLUSTERED,
    PositionName NVARCHAR(100) NOT NULL CONSTRAINT CK_Positions_Name CHECK (LEN(TRIM(PositionName)) > 0 AND PositionName = LTRIM(RTRIM(PositionName))),
    IsDeleted BIT CONSTRAINT DF_Positions_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Positions_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL
) WITH (DATA_COMPRESSION = PAGE);

CREATE UNIQUE NONCLUSTERED INDEX IX_Positions_Name ON [Reference].[Positions](PositionName) WHERE IsDeleted = 0;

CREATE TABLE [Reference].[Qualifications] (
    QualificationId UNIQUEIDENTIFIER CONSTRAINT DF_Qualifications_QualificationId DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Qualifications PRIMARY KEY CLUSTERED,
    QualificationName NVARCHAR(100) NOT NULL CONSTRAINT CK_Qualifications_Name CHECK (LEN(TRIM(QualificationName)) > 0 AND QualificationName = LTRIM(RTRIM(QualificationName))),
    IsDeleted BIT CONSTRAINT DF_Qualifications_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Qualifications_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL
) WITH (DATA_COMPRESSION = PAGE);

CREATE UNIQUE NONCLUSTERED INDEX IX_Qualifications_Name ON [Reference].[Qualifications](QualificationName) WHERE IsDeleted = 0;

CREATE TABLE [Reference].[PedagogicalTitles] (
    TitleId UNIQUEIDENTIFIER CONSTRAINT DF_PedagogicalTitles_TitleId DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_PedagogicalTitles PRIMARY KEY CLUSTERED,
    TitleName NVARCHAR(100) NOT NULL CONSTRAINT CK_PedagogicalTitles_Name CHECK (LEN(TRIM(TitleName)) > 0 AND TitleName = LTRIM(RTRIM(TitleName))),
    IsDeleted BIT CONSTRAINT DF_PedagogicalTitles_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_PedagogicalTitles_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL
) WITH (DATA_COMPRESSION = PAGE);

CREATE UNIQUE NONCLUSTERED INDEX IX_PedagogicalTitles_Name ON [Reference].[PedagogicalTitles](TitleName) WHERE IsDeleted = 0;

CREATE TABLE [Reference].[GradeTypes] (
    GradeTypeId UNIQUEIDENTIFIER CONSTRAINT DF_GradeTypes_Id DEFAULT NEWSEQUENTIALID() CONSTRAINT PK_GradeTypes PRIMARY KEY CLUSTERED,
    TypeName NVARCHAR(50) NOT NULL CONSTRAINT CK_GradeTypes_TypeName CHECK (LEN(TRIM(TypeName)) > 0 AND TypeName = LTRIM(RTRIM(TypeName))),
    IsDeleted BIT CONSTRAINT DF_GradeTypes_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_GradeTypes_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL
) WITH (DATA_COMPRESSION = PAGE);

CREATE UNIQUE NONCLUSTERED INDEX IX_GradeTypes_TypeName ON [Reference].[GradeTypes](TypeName) WHERE IsDeleted = 0;

CREATE TABLE [Reference].[LessonTypes] (
    LessonTypeId UNIQUEIDENTIFIER CONSTRAINT DF_LessonTypes_Id DEFAULT NEWSEQUENTIALID() CONSTRAINT PK_LessonTypes PRIMARY KEY CLUSTERED,
    TypeName NVARCHAR(50) NOT NULL CONSTRAINT CK_LessonTypes_TypeName CHECK (LEN(TRIM(TypeName)) > 0 AND TypeName = LTRIM(RTRIM(TypeName))),
    IsDeleted BIT CONSTRAINT DF_LessonTypes_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_LessonTypes_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL
) WITH (DATA_COMPRESSION = PAGE);

CREATE UNIQUE NONCLUSTERED INDEX IX_LessonTypes_TypeName ON [Reference].[LessonTypes](TypeName) WHERE IsDeleted = 0;

CREATE TABLE [Reference].[BellSchedules] (
    ScheduleId UNIQUEIDENTIFIER CONSTRAINT DF_BellSchedules_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_BellSchedules PRIMARY KEY CLUSTERED,
    LessonNumber INT NOT NULL CONSTRAINT CK_BellSchedules_LessonNumber CHECK (LessonNumber > 0),
    StartTime DATETIMEOFFSET NOT NULL,
    EndTime DATETIMEOFFSET NOT NULL,
    IsDeleted BIT CONSTRAINT DF_BellSchedules_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_BellSchedules_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    CONSTRAINT CK_BellSchedule_Times CHECK (StartTime < EndTime)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_BellSchedules_IsDeleted ON [Reference].[BellSchedules](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE UNIQUE NONCLUSTERED INDEX IX_BellSchedules_LessonNumber ON [Reference].[BellSchedules](LessonNumber) WHERE IsDeleted = 0;

CREATE TABLE [Reference].[Classrooms] (
    RoomId UNIQUEIDENTIFIER CONSTRAINT DF_Classrooms_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Classrooms PRIMARY KEY CLUSTERED,
    RoomNumber NVARCHAR(20) NOT NULL CONSTRAINT CK_Classrooms_RoomNumber CHECK (LEN(TRIM(RoomNumber)) > 0 AND RoomNumber = LTRIM(RTRIM(RoomNumber))),
    Name NVARCHAR(100) NULL,
    Capacity INT CONSTRAINT DF_Classrooms_Capacity DEFAULT 30 NOT NULL CONSTRAINT CK_Classrooms_Capacity CHECK (Capacity > 0 AND Capacity <= 200),
    IsDeleted BIT CONSTRAINT DF_Classrooms_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Classrooms_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL
) WITH (DATA_COMPRESSION = PAGE);

CREATE UNIQUE NONCLUSTERED INDEX IX_Classrooms_RoomNumber ON [Reference].[Classrooms](RoomNumber) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Classrooms_IsDeleted ON [Reference].[Classrooms](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Communications].[Announcements] (
    AnnouncementId UNIQUEIDENTIFIER CONSTRAINT DF_Announcements_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Announcements PRIMARY KEY CLUSTERED,
    Title NVARCHAR(150) NOT NULL CONSTRAINT CK_Announcements_Title CHECK (LEN(TRIM(Title)) > 0 AND Title = LTRIM(RTRIM(Title))),
    Content NVARCHAR(MAX) NOT NULL CONSTRAINT CK_Announcements_Content CHECK (LEN(TRIM(Content)) > 0 AND Content = LTRIM(RTRIM(Content))),
    AuthorId UNIQUEIDENTIFIER NOT NULL,
    DateCreated DATETIMEOFFSET CONSTRAINT DF_Announcements_DateCreated DEFAULT GETUTCDATE() NOT NULL,
    ExpirationDate DATETIMEOFFSET NULL,
    IsActive BIT CONSTRAINT DF_Announcements_IsActive DEFAULT 1 NOT NULL,
    IsDeleted BIT CONSTRAINT DF_Announcements_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Announcements_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CK_Announcements_Dates CHECK (ExpirationDate IS NULL OR ExpirationDate > DateCreated)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Announcements_IsDeleted ON [Communications].[Announcements](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Infrastructure].[OutboxMessages] (
    Id UNIQUEIDENTIFIER CONSTRAINT DF_Outbox_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_OutboxMessages PRIMARY KEY CLUSTERED,
    CreatedByUserId UNIQUEIDENTIFIER NOT NULL,
    Type NVARCHAR(50) NOT NULL CONSTRAINT CK_Outbox_Type CHECK (LEN(TRIM(Type)) > 0 AND Type = LTRIM(RTRIM(Type))),
    Content NVARCHAR(4000) NOT NULL CONSTRAINT CK_Outbox_Content CHECK (ISJSON(Content) = 1 AND LEN(TRIM(Content)) > 0 AND Content = LTRIM(RTRIM(Content))),
    OccurredOnUtc DATETIMEOFFSET NOT NULL,
    ProcessedOnUtc DATETIMEOFFSET NULL,
    Error NVARCHAR(4000) NULL,
    IsDeleted BIT CONSTRAINT DF_Outbox_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Outbox_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    CONSTRAINT CK_OutboxMessages_Dates CHECK (ProcessedOnUtc IS NULL OR ProcessedOnUtc >= OccurredOnUtc)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_OutboxMessages_IsDeleted ON [Infrastructure].[OutboxMessages](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Core].[Teachers] (
    TeacherId UNIQUEIDENTIFIER CONSTRAINT DF_Teachers_TeacherId DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Teachers PRIMARY KEY CLUSTERED,
    LastName NVARCHAR(50) NOT NULL CONSTRAINT CK_Teachers_LastName CHECK (LEN(TRIM(LastName)) > 0 AND LastName = LTRIM(RTRIM(LastName))),
    FirstName NVARCHAR(50) NOT NULL CONSTRAINT CK_Teachers_FirstName CHECK (LEN(TRIM(FirstName)) > 0 AND FirstName = LTRIM(RTRIM(FirstName))),
    MiddleName NVARCHAR(50) NULL,
    Phone NVARCHAR(20) MASKED WITH (FUNCTION = 'default()') NULL,
    Specialization NVARCHAR(100) NULL,
    DateOfBirth DATETIMEOFFSET NULL,
    Gender NVARCHAR(10) NOT NULL CONSTRAINT CK_Teachers_Gender CHECK (Gender IN ('Male', 'Female') AND Gender = LTRIM(RTRIM(Gender))),
    Workload DECIMAL(5,2) NULL CONSTRAINT CK_Teachers_Workload CHECK (Workload >= 0 AND Workload <= 2.0),
    EducationInfo NVARCHAR(1000) NULL,
    MeetLink NVARCHAR(255) NULL, 
    UserId UNIQUEIDENTIFIER NULL,
    PositionId UNIQUEIDENTIFIER NOT NULL,
    QualificationId UNIQUEIDENTIFIER NOT NULL, 
    PedagogicalTitleId UNIQUEIDENTIFIER NULL,
    IsActive BIT CONSTRAINT DF_Teachers_IsActive DEFAULT 1 NOT NULL,
    IsDeleted BIT CONSTRAINT DF_Teachers_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Teachers_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Teachers_Users FOREIGN KEY (UserId) REFERENCES [Identity].[Users](UserId),
    CONSTRAINT FK_Teachers_Positions FOREIGN KEY (PositionId) REFERENCES [Reference].[Positions](PositionId),
    CONSTRAINT FK_Teachers_Qualifications FOREIGN KEY (QualificationId) REFERENCES [Reference].[Qualifications](QualificationId),
    CONSTRAINT FK_Teachers_PedagogicalTitles FOREIGN KEY (PedagogicalTitleId) REFERENCES [Reference].[PedagogicalTitles](TitleId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Teachers_IsDeleted ON [Core].[Teachers](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE UNIQUE NONCLUSTERED INDEX IX_Teachers_Phone ON [Core].[Teachers](Phone) WHERE Phone IS NOT NULL AND IsDeleted = 0;

CREATE TABLE [Identity].[RefreshTokens] (
    TokenId UNIQUEIDENTIFIER CONSTRAINT DF_RefreshTokens_TokenId DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_RefreshTokens PRIMARY KEY CLUSTERED,
    UserId UNIQUEIDENTIFIER NOT NULL,
    TokenHash NVARCHAR(100) NOT NULL CONSTRAINT CK_Refresh_TokenHash CHECK (LEN(TRIM(TokenHash)) > 0 AND TokenHash = LTRIM(RTRIM(TokenHash))),
    ExpiresAt DATETIMEOFFSET NOT NULL,
    CreatedByIp NVARCHAR(45) NULL,
    DeviceIdentifier NVARCHAR(128) NULL,
    Revoked BIT CONSTRAINT DF_RefreshTokens_Revoked DEFAULT 0 NOT NULL,
    RevokedAt DATETIMEOFFSET NULL,
    ReplacedByTokenHash NVARCHAR(100) NULL,
    IsDeleted BIT CONSTRAINT DF_RefreshTokens_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_RefreshTokens_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CK_RefreshTokens_Timeline CHECK (ExpiresAt > CreatedAt AND (RevokedAt IS NULL OR RevokedAt >= CreatedAt)),
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES [Identity].[Users](UserId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_RefreshTokens_IsDeleted ON [Identity].[RefreshTokens](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE UNIQUE NONCLUSTERED INDEX IX_RefreshTokens_TokenHash ON [Identity].[RefreshTokens](TokenHash) WHERE IsDeleted = 0;

CREATE TABLE [Core].[Subjects] (
    SubjectId UNIQUEIDENTIFIER CONSTRAINT DF_Subjects_SubjectId DEFAULT NEWSEQUENTIALID() CONSTRAINT PK_Subjects PRIMARY KEY CLUSTERED,
    SubjectName NVARCHAR(100) NOT NULL CONSTRAINT CK_Subjects_SubjectName CHECK (LEN(TRIM(SubjectName)) > 0 AND SubjectName = LTRIM(RTRIM(SubjectName))),
    IsDeleted BIT CONSTRAINT DF_Subjects_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Subjects_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL
) WITH (DATA_COMPRESSION = PAGE);

CREATE UNIQUE NONCLUSTERED INDEX IX_Subjects_SubjectName ON [Core].[Subjects](SubjectName) WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Positions_IsDeleted ON [Reference].[Positions](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Roles_IsDeleted ON [Identity].[Roles](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Semesters_IsDeleted ON [Reference].[Semesters](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_SystemSettings_IsDeleted ON [Infrastructure].[SystemSettings](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Qualifications_IsDeleted ON [Reference].[Qualifications](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_PedagogicalTitles_IsDeleted ON [Reference].[PedagogicalTitles](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_GradeTypes_IsDeleted ON [Reference].[GradeTypes](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_LessonTypes_IsDeleted ON [Reference].[LessonTypes](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Subjects_IsDeleted ON [Core].[Subjects](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Core].[Classes] (
    ClassId UNIQUEIDENTIFIER CONSTRAINT DF_Classes_ClassId DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Classes PRIMARY KEY CLUSTERED,
    ClassName NVARCHAR(10) NOT NULL CONSTRAINT CK_Classes_ClassName CHECK (LEN(TRIM(ClassName)) > 0 AND ClassName = LTRIM(RTRIM(ClassName))),
    GradeLevel INT NOT NULL CONSTRAINT CK_Classes_GradeLevel CHECK (GradeLevel BETWEEN 1 AND 12),
    AcademicYear NVARCHAR(20) NOT NULL CONSTRAINT CK_Classes_AcademicYear CHECK (LEN(TRIM(AcademicYear)) > 0 AND AcademicYear = LTRIM(RTRIM(AcademicYear))),
    HomeroomTeacherId UNIQUEIDENTIFIER NOT NULL,
    IsActive BIT CONSTRAINT DF_Classes_IsActive DEFAULT 1 NOT NULL,
    IsDeleted BIT CONSTRAINT DF_Classes_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Classes_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Classes_Teachers FOREIGN KEY (HomeroomTeacherId) REFERENCES [Core].[Teachers](TeacherId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Classes_IsDeleted ON [Core].[Classes](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Core].[Students] (
    StudentId UNIQUEIDENTIFIER CONSTRAINT DF_Students_StudentId DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Students PRIMARY KEY CLUSTERED,
    LastName NVARCHAR(50) NOT NULL CONSTRAINT CK_Students_LastName CHECK (LEN(TRIM(LastName)) > 0 AND LastName = LTRIM(RTRIM(LastName))),
    FirstName NVARCHAR(50) NOT NULL CONSTRAINT CK_Students_FirstName CHECK (LEN(TRIM(FirstName)) > 0 AND FirstName = LTRIM(RTRIM(FirstName))),
    MiddleName NVARCHAR(50) NULL,
    DateOfBirth DATETIMEOFFSET NULL,
    ClassId UNIQUEIDENTIFIER NOT NULL,
    Gender NVARCHAR(10) NULL CONSTRAINT CK_Students_Gender CHECK (Gender IN ('Male', 'Female')),
    DocumentType NVARCHAR(50) NULL CONSTRAINT CK_Students_DocumentType CHECK (DocumentType IS NULL OR (LEN(TRIM(DocumentType)) > 0 AND DocumentType = LTRIM(RTRIM(DocumentType)))),
    DocumentSeries NVARCHAR(10) NULL,
    DocumentNumber NVARCHAR(20) NULL,
    EnrollmentDate DATETIMEOFFSET NULL,
    EnrollmentReason NVARCHAR(200) NULL,
    Address NVARCHAR(500) MASKED WITH (FUNCTION = 'default()') NULL,
    MedicalNotes NVARCHAR(2000) NULL,
    UserId UNIQUEIDENTIFIER NULL,
    IsActive BIT CONSTRAINT DF_Students_IsActive DEFAULT 1 NOT NULL,
    IsDeleted BIT CONSTRAINT DF_Students_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Students_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CK_Students_Dates CHECK (EnrollmentDate IS NULL OR DateOfBirth IS NULL OR EnrollmentDate >= DateOfBirth),
    CONSTRAINT CK_Students_EnrollmentPast CHECK (EnrollmentDate IS NULL OR EnrollmentDate <= GETUTCDATE()),
    CONSTRAINT CK_Students_Timeline CHECK (UpdatedAt IS NULL OR UpdatedAt >= CreatedAt),
    CONSTRAINT FK_Students_Classes FOREIGN KEY (ClassId) REFERENCES [Core].[Classes](ClassId),
    CONSTRAINT FK_Students_Users FOREIGN KEY (UserId) REFERENCES [Identity].[Users](UserId),
    SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START HIdDEN NOT NULL,
    SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END HIdDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime)
) WITH (DATA_COMPRESSION = PAGE, SYSTEM_VERSIONING = ON (HISTORY_TABLE = [Core].[Students_EFMigrationsHistory]));

CREATE NONCLUSTERED INDEX IX_Students_IsDeleted ON [Core].[Students](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Core].[Parents] (
    ParentId UNIQUEIDENTIFIER CONSTRAINT DF_Parents_ParentId DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Parents PRIMARY KEY CLUSTERED,
    LastName NVARCHAR(50) NULL,
    FirstName NVARCHAR(50) NULL,
    MiddleName NVARCHAR(50) NULL,
    Phone NVARCHAR(20) MASKED WITH (FUNCTION = 'default()') NULL,           
    UserId UNIQUEIDENTIFIER NULL,
    IsActive BIT CONSTRAINT DF_Parents_IsActive DEFAULT 1 NOT NULL,
    IsDeleted BIT CONSTRAINT DF_Parents_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Parents_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Parents_Users FOREIGN KEY (UserId) REFERENCES [Identity].[Users](UserId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Parents_IsDeleted ON [Core].[Parents](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE UNIQUE NONCLUSTERED INDEX IX_Parents_Phone ON [Core].[Parents](Phone) WHERE IsDeleted = 0 AND Phone IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX IX_Parents_UserId ON [Core].[Parents](UserId) WHERE UserId IS NOT NULL WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Core].[StudentParents] (
    StudentParentId UNIQUEIDENTIFIER CONSTRAINT DF_StudentParents_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_StudentParents PRIMARY KEY CLUSTERED,
    StudentId UNIQUEIDENTIFIER NOT NULL,
    ParentId UNIQUEIDENTIFIER NOT NULL,
    Role NVARCHAR(50) NULL,
    IsDeleted BIT CONSTRAINT DF_StudentParents_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_StudentParents_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    CONSTRAINT FK_StudentParents_Students FOREIGN KEY (StudentId) REFERENCES [Core].[Students](StudentId),
    CONSTRAINT FK_StudentParents_Parents FOREIGN KEY (ParentId) REFERENCES [Core].[Parents](ParentId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_StudentParents_IsDeleted ON [Core].[StudentParents](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Core].[Subgroups] (
    SubgroupId UNIQUEIDENTIFIER CONSTRAINT DF_Subgroups_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Subgroups PRIMARY KEY CLUSTERED,
    ClassId UNIQUEIDENTIFIER NOT NULL, 
    SubjectId UNIQUEIDENTIFIER NOT NULL,
    SubgroupName NVARCHAR(50) NOT NULL CONSTRAINT CK_Subgroups_SubgroupName CHECK (LEN(TRIM(SubgroupName)) > 0 AND SubgroupName = LTRIM(RTRIM(SubgroupName))),
    IsActive BIT CONSTRAINT DF_Subgroups_IsActive DEFAULT 1 NOT NULL,
    IsDeleted BIT CONSTRAINT DF_Subgroups_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Subgroups_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Subgroups_Classes FOREIGN KEY (ClassId) REFERENCES [Core].[Classes](ClassId),
    CONSTRAINT FK_Subgroups_Subjects FOREIGN KEY (SubjectId) REFERENCES [Core].[Subjects](SubjectId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Subgroups_IsDeleted ON [Core].[Subgroups](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Core].[StudentSubgroups] (
    StudentSubgroupId UNIQUEIDENTIFIER CONSTRAINT DF_StudentSubgroups_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_StudentSubgroups PRIMARY KEY CLUSTERED,
    StudentId UNIQUEIDENTIFIER NOT NULL,
    SubgroupId UNIQUEIDENTIFIER NOT NULL,
    IsDeleted BIT CONSTRAINT DF_StudentSubgroups_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_StudentSubgroups_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    CONSTRAINT FK_StudentSubgroups_Students FOREIGN KEY (StudentId) REFERENCES [Core].[Students](StudentId),
    CONSTRAINT FK_StudentSubgroups_Subgroups FOREIGN KEY (SubgroupId) REFERENCES [Core].[Subgroups](SubgroupId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_StudentSubgroups_IsDeleted ON [Core].[StudentSubgroups](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE UNIQUE NONCLUSTERED INDEX IX_StudentSubgroups_Student_Subgroup ON [Core].[StudentSubgroups](StudentId, SubgroupId) WHERE IsDeleted = 0;
CREATE UNIQUE NONCLUSTERED INDEX IX_StudentParents_Student_Parent ON [Core].[StudentParents](StudentId, ParentId) WHERE IsDeleted = 0;

CREATE TABLE [Operations].[TeachingAssignments] (
    AssignmentId UNIQUEIDENTIFIER CONSTRAINT DF_TeachingAssignments_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_TeachingAssignments PRIMARY KEY CLUSTERED,
    TeacherId UNIQUEIDENTIFIER NOT NULL,
    SubjectId UNIQUEIDENTIFIER NOT NULL,
    ClassId UNIQUEIDENTIFIER NOT NULL,
    SubgroupId UNIQUEIDENTIFIER NULL,
    IsActive BIT CONSTRAINT DF_TeachingAssignments_IsActive DEFAULT 1 NOT NULL,
    IsDeleted BIT CONSTRAINT DF_TeachingAssignments_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_TeachingAssignments_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_TeachingAssignments_Teachers FOREIGN KEY (TeacherId) REFERENCES [Core].[Teachers](TeacherId),
    CONSTRAINT FK_TeachingAssignments_Subjects FOREIGN KEY (SubjectId) REFERENCES [Core].[Subjects](SubjectId),
    CONSTRAINT FK_TeachingAssignments_Classes FOREIGN KEY (ClassId) REFERENCES [Core].[Classes](ClassId),
    CONSTRAINT FK_TeachingAssignments_Subgroups FOREIGN KEY (SubgroupId) REFERENCES [Core].[Subgroups](SubgroupId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_TeachingAssignments_IsDeleted ON [Operations].[TeachingAssignments](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Operations].[FixedSchedules] (
    ScheduleId UNIQUEIDENTIFIER CONSTRAINT DF_FixedSchedules_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_FixedSchedules PRIMARY KEY CLUSTERED,
    DayOfWeek INT NOT NULL CONSTRAINT CK_FixedSchedules_DayOfWeek CHECK (DayOfWeek BETWEEN 1 AND 7),
    PeriodId UNIQUEIDENTIFIER NOT NULL,
    AssignmentId UNIQUEIDENTIFIER NOT NULL,
    RoomId UNIQUEIDENTIFIER NOT NULL,
    IsDeleted BIT CONSTRAINT DF_FixedSchedules_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_FixedSchedules_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_FixedSchedules_TeachingAssignments FOREIGN KEY (AssignmentId) REFERENCES [Operations].[TeachingAssignments](AssignmentId),
    CONSTRAINT FK_FixedSchedules_Classrooms FOREIGN KEY (RoomId) REFERENCES [Reference].[Classrooms](RoomId),
    CONSTRAINT FK_FixedSchedules_BellSchedules FOREIGN KEY (PeriodId) REFERENCES [Reference].[BellSchedules](ScheduleId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_FixedSchedules_IsDeleted ON [Operations].[FixedSchedules](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Operations].[Lessons] (
    LessonId UNIQUEIDENTIFIER CONSTRAINT DF_Lessons_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Lessons PRIMARY KEY CLUSTERED,
    AssignmentId UNIQUEIDENTIFIER NOT NULL,
    LessonDate DATETIMEOFFSET NOT NULL,
    LessonTopic NVARCHAR(255) NULL,
    Homework NVARCHAR(1000) NULL,
    LessonTypeId UNIQUEIDENTIFIER NOT NULL,
    PeriodId UNIQUEIDENTIFIER NOT NULL,
    RoomId UNIQUEIDENTIFIER NOT NULL,
    SemesterId UNIQUEIDENTIFIER NOT NULL,
    IsDeleted BIT CONSTRAINT DF_Lessons_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Lessons_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Lessons_TeachingAssignments FOREIGN KEY (AssignmentId) REFERENCES [Operations].[TeachingAssignments](AssignmentId),
    CONSTRAINT FK_Lessons_LessonTypes FOREIGN KEY (LessonTypeId) REFERENCES [Reference].[LessonTypes](LessonTypeId),
    CONSTRAINT FK_Lessons_Classrooms FOREIGN KEY (RoomId) REFERENCES [Reference].[Classrooms](RoomId),
    CONSTRAINT FK_Lessons_Semesters FOREIGN KEY (SemesterId) REFERENCES [Reference].[Semesters](SemesterId),
    CONSTRAINT FK_Lessons_BellSchedules FOREIGN KEY (PeriodId) REFERENCES [Reference].[BellSchedules](ScheduleId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Lessons_IsDeleted ON [Operations].[Lessons](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Operations].[Grades] (
    GradeId UNIQUEIDENTIFIER CONSTRAINT DF_Grades_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Grades PRIMARY KEY CLUSTERED,
    LessonId UNIQUEIDENTIFIER NOT NULL,
    StudentId UNIQUEIDENTIFIER NOT NULL,
    GradeValue NVARCHAR(3) NOT NULL CONSTRAINT CK_Grades_GradeValue CHECK (LEN(TRIM(GradeValue)) > 0 AND GradeValue = LTRIM(RTRIM(GradeValue)) AND (TRY_CAST(GradeValue AS INT) BETWEEN 1 AND 12 OR GradeValue IN (N'Н', N'хв'))),
    Comment NVARCHAR(255) NULL,
    CreatedByUserId UNIQUEIDENTIFIER NOT NULL,
    UpdatedByUserId UNIQUEIDENTIFIER NOT NULL,
    GradeTypeId UNIQUEIDENTIFIER NOT NULL,
    IsDeleted BIT CONSTRAINT DF_Grades_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Grades_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START HIdDEN NOT NULL,
    SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END HIdDEN NOT NULL,
    PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime),
    CONSTRAINT FK_Grades_Lessons FOREIGN KEY (LessonId) REFERENCES [Operations].[Lessons](LessonId),
    CONSTRAINT FK_Grades_Students FOREIGN KEY (StudentId) REFERENCES [Core].[Students](StudentId),
    CONSTRAINT FK_Grades_GradeTypes FOREIGN KEY (GradeTypeId) REFERENCES [Reference].[GradeTypes](GradeTypeId),
    CONSTRAINT FK_Grades_Users_Created FOREIGN KEY (CreatedByUserId) REFERENCES [Identity].[Users](UserId),
    CONSTRAINT FK_Grades_Users_Updated FOREIGN KEY (UpdatedByUserId) REFERENCES [Identity].[Users](UserId)
) WITH (DATA_COMPRESSION = PAGE, SYSTEM_VERSIONING = ON (HISTORY_TABLE = [Operations].[Grades_EFMigrationsHistory]));

CREATE NONCLUSTERED INDEX IX_Grades_IsDeleted ON [Operations].[Grades](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Operations].[Attendances] (
    AttendanceId UNIQUEIDENTIFIER CONSTRAINT DF_Attendances_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Attendances PRIMARY KEY CLUSTERED,
    LessonId UNIQUEIDENTIFIER NOT NULL,
    StudentId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT CK_Attendances_Status CHECK (LEN(TRIM(Status)) > 0 AND Status = LTRIM(RTRIM(Status))),
    Comment NVARCHAR(255) NULL,
    IsDeleted BIT CONSTRAINT DF_Attendances_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Attendances_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Attendances_Lessons FOREIGN KEY (LessonId) REFERENCES [Operations].[Lessons](LessonId),
    CONSTRAINT FK_Attendances_Students FOREIGN KEY (StudentId) REFERENCES [Core].[Students](StudentId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Attendances_IsDeleted ON [Operations].[Attendances](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Operations].[TeacherSubstitutions] (
    SubstitutionId UNIQUEIDENTIFIER CONSTRAINT DF_TeacherSubstitutions_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_TeacherSubstitutions PRIMARY KEY CLUSTERED,
    AssignmentId UNIQUEIDENTIFIER NOT NULL,        
    SubstituteTeacherId UNIQUEIDENTIFIER NOT NULL,
    StartDate DATETIMEOFFSET NOT NULL,            
    EndDate DATETIMEOFFSET NOT NULL,               
    IsDeleted BIT CONSTRAINT DF_TeacherSubstitutions_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_TeacherSubstitutions_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CK_TeacherSubstitutions_Dates CHECK (StartDate <= EndDate),
    CONSTRAINT FK_TeacherSubstitutions_TeachingAssignments FOREIGN KEY (AssignmentId) REFERENCES [Operations].[TeachingAssignments](AssignmentId),
    CONSTRAINT FK_TeacherSubstitutions_Teachers FOREIGN KEY (SubstituteTeacherId) REFERENCES [Core].[Teachers](TeacherId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_TeacherSubstitutions_IsDeleted ON [Operations].[TeacherSubstitutions](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Operations].[Quizzes] (
    QuizId UNIQUEIDENTIFIER CONSTRAINT DF_Quizzes_QuizId DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_Quizzes PRIMARY KEY CLUSTERED,
    TeacherId UNIQUEIDENTIFIER NOT NULL,              
    SubjectId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(255) NOT NULL CONSTRAINT CK_Quizzes_Title CHECK (LEN(TRIM(Title)) > 0 AND Title = LTRIM(RTRIM(Title))),
    IsDeleted BIT CONSTRAINT DF_Quizzes_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Quizzes_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Quizzes_Teachers FOREIGN KEY (TeacherId) REFERENCES [Core].[Teachers](TeacherId),
    CONSTRAINT FK_Quizzes_Subjects FOREIGN KEY (SubjectId) REFERENCES [Core].[Subjects](SubjectId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Quizzes_IsDeleted ON [Operations].[Quizzes](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Quizzes_TeacherId ON [Operations].[Quizzes](TeacherId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Quizzes_SubjectId ON [Operations].[Quizzes](SubjectId) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Operations].[QuizQuestions] (
    QuestionId UNIQUEIDENTIFIER CONSTRAINT DF_QuizQuestions_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_QuizQuestions PRIMARY KEY CLUSTERED,
    QuizId UNIQUEIDENTIFIER NOT NULL,
    OrderIndex INT CONSTRAINT DF_QuizQuestions_OrderIndex DEFAULT 0 NOT NULL CONSTRAINT CK_QuizQuestions_OrderIndex CHECK (OrderIndex >= 0),
    QuestionText NVARCHAR(2000) NOT NULL CONSTRAINT CK_QuizQuestions_Text CHECK (LEN(TRIM(QuestionText)) > 0 AND QuestionText = LTRIM(RTRIM(QuestionText))), 
    QuestionType INT CONSTRAINT DF_QuizQuestions_Type DEFAULT 0 NOT NULL CONSTRAINT CK_QuizQuestions_QuestionType CHECK (QuestionType >= 0),
    ContentJson NVARCHAR(MAX) NOT NULL CONSTRAINT CK_QuizQuestions_Json CHECK (ISJSON(ContentJson) = 1 AND LEN(TRIM(ContentJson)) > 0 AND ContentJson = LTRIM(RTRIM(ContentJson))),
    Points INT CONSTRAINT DF_QuizQuestions_Points DEFAULT 1 NOT NULL CONSTRAINT CK_QuizQuestions_Points CHECK (Points >= 0),
    IsDeleted BIT CONSTRAINT DF_QuizQuestions_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_QuizQuestions_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_QuizQuestions_Quizzes FOREIGN KEY (QuizId) REFERENCES [Operations].[Quizzes](QuizId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_QuizQuestions_QuizId_Active ON [Operations].[QuizQuestions] (QuizId) WHERE IsDeleted = 0 WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Operations].[QuizAssignments] (
    AssignmentId UNIQUEIDENTIFIER CONSTRAINT DF_QuizAssignments_Id DEFAULT NEWSEQUENTIALId() CONSTRAINT PK_QuizAssignments PRIMARY KEY CLUSTERED,
    QuizId UNIQUEIDENTIFIER NOT NULL,
    ClassId UNIQUEIDENTIFIER NOT NULL,
    AssignedDate DATETIMEOFFSET CONSTRAINT DF_QuizAssignments_Assigned DEFAULT GETUTCDATE() NOT NULL,
    DueDate DATETIMEOFFSET NULL,
    IsDeleted BIT CONSTRAINT DF_QuizAssignments_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_QuizAssignments_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CK_QuizAssignments_Dates CHECK (DueDate IS NULL OR DueDate >= AssignedDate),
    CONSTRAINT FK_QuizAssignments_Quizzes FOREIGN KEY (QuizId) REFERENCES [Operations].[Quizzes](QuizId),
    CONSTRAINT FK_QuizAssignments_Classes FOREIGN KEY (ClassId) REFERENCES [Core].[Classes](ClassId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_QuizAssignments_IsDeleted ON [Operations].[QuizAssignments](IsDeleted) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Operations].[QuizSubmissions] (
    SubmissionId UNIQUEIDENTIFIER CONSTRAINT DF_QuizSubmissions_Id DEFAULT NEWSEQUENTIALID() CONSTRAINT PK_QuizSubmissions PRIMARY KEY CLUSTERED,
    AssignmentId UNIQUEIDENTIFIER NOT NULL,
    StudentId UNIQUEIDENTIFIER NOT NULL,
    Score INT NOT NULL CONSTRAINT CK_QuizSubmissions_Score CHECK (Score >= 0),
    MaxScore INT NOT NULL CONSTRAINT CK_QuizSubmissions_MaxScore CHECK (MaxScore > 0),
    IsDeleted BIT CONSTRAINT DF_QuizSubmissions_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_QuizSubmissions_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CK_QuizSubmissions_ScoreMax CHECK (Score <= MaxScore),
    CONSTRAINT FK_QuizSubmissions_Assignments FOREIGN KEY (AssignmentId) REFERENCES [Operations].[QuizAssignments](AssignmentId),
    CONSTRAINT FK_QuizSubmissions_Students FOREIGN KEY (StudentId) REFERENCES [Core].[Students](StudentId),
    CONSTRAINT UQ_QuizSubmissions_Student_Assignment UNIQUE (StudentId, AssignmentId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_QuizSubmissions_IsDeleted ON [Operations].[QuizSubmissions](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_QuizSubmissions_StudentId ON [Operations].[QuizSubmissions](StudentId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_QuizSubmissions_AssignmentId ON [Operations].[QuizSubmissions](AssignmentId) WITH (DATA_COMPRESSION = PAGE);

PRINT N'=== Додавання системи LunarCoins (Гейміфікація) ===';

CREATE TABLE [Operations].[Wallets] (
    WalletId UNIQUEIDENTIFIER CONSTRAINT DF_Wallets_Id DEFAULT NEWSEQUENTIALID() CONSTRAINT PK_Wallets PRIMARY KEY CLUSTERED,
    StudentId UNIQUEIDENTIFIER NOT NULL,
    SubjectId UNIQUEIDENTIFIER NOT NULL,
    Balance INT CONSTRAINT DF_Wallets_Balance DEFAULT 0 NOT NULL CONSTRAINT CK_Wallets_Balance CHECK (Balance >= 0),
    IsDeleted BIT CONSTRAINT DF_Wallets_IsDeleted DEFAULT 0 NOT NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_Wallets_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Wallets_Students FOREIGN KEY (StudentId) REFERENCES [Core].[Students](StudentId),
    CONSTRAINT FK_Wallets_Subjects FOREIGN KEY (SubjectId) REFERENCES [Core].[Subjects](SubjectId),
    CONSTRAINT UQ_Wallets_Student_Subject UNIQUE (StudentId, SubjectId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE TABLE [Operations].[CoinTransactions] (
    TransactionId UNIQUEIDENTIFIER CONSTRAINT DF_CoinTransactions_Id DEFAULT NEWSEQUENTIALID() CONSTRAINT PK_CoinTransactions PRIMARY KEY CLUSTERED,
    WalletId UNIQUEIDENTIFIER NOT NULL,
    Amount INT NOT NULL CONSTRAINT CK_CoinTransactions_Amount CHECK (Amount <> 0),
    ReferenceId UNIQUEIDENTIFIER NOT NULL, -- GradeId або QuizSubmissionId
    TransactionType NVARCHAR(50) NOT NULL CONSTRAINT CK_CoinTransactions_Type CHECK (TransactionType IN (N'Earned_Grade', N'Earned_Quiz', N'Spent_GradeBoost')),
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_CoinTransactions_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    CONSTRAINT FK_CoinTransactions_Wallets FOREIGN KEY (WalletId) REFERENCES [Operations].[Wallets](WalletId)
) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Wallets_StudentId ON [Operations].[Wallets](StudentId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_CoinTransactions_WalletId ON [Operations].[CoinTransactions](WalletId) WITH (DATA_COMPRESSION = PAGE);
GO

CREATE SEQUENCE [Infrastructure].[Sq_AuditLogs]
    AS BIGINT
    START WITH -9223372036854775808 
    INCREMENT BY 1
    CACHE 1000;
GO

CREATE TABLE [Infrastructure].[AuditLogs] (
    AuditId BIGINT CONSTRAINT DF_AuditLogs_Id DEFAULT (NEXT VALUE FOR [Infrastructure].[Sq_AuditLogs]) CONSTRAINT PK_AuditLogs PRIMARY KEY CLUSTERED,
    UserId UNIQUEIDENTIFIER NULL,
    EntityName NVARCHAR(100) NOT NULL CONSTRAINT CK_Audit_EntityName CHECK (LEN(TRIM(EntityName)) > 0 AND EntityName = LTRIM(RTRIM(EntityName))),
    EntityRef NVARCHAR(100) NOT NULL CONSTRAINT CK_Audit_EntityId CHECK (LEN(TRIM(EntityRef)) > 0 AND EntityRef = LTRIM(RTRIM(EntityRef))),
    Action NVARCHAR(20) NOT NULL CONSTRAINT CK_Audit_Action CHECK (LEN(TRIM(Action)) > 0 AND Action = LTRIM(RTRIM(Action))), 
    OldValue NVARCHAR(MAX) NULL CONSTRAINT CK_Audit_OldValue CHECK (ISJSON(OldValue) = 1),
    NewValue NVARCHAR(MAX) NULL CONSTRAINT CK_Audit_NewValue CHECK (ISJSON(NewValue) = 1),
    OccurredAtUtc DATETIMEOFFSET CONSTRAINT DF_AuditLogs_Occurred DEFAULT GETUTCDATE() NOT NULL,
    ClientIp NVARCHAR(45) NULL,
    CreatedAt DATETIMEOFFSET CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT GETUTCDATE() NOT NULL,
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES [Identity].[Users](UserId)
) WITH (DATA_COMPRESSION = PAGE,
    LEDGER = ON (APPEND_ONLY = ON));

CREATE NONCLUSTERED INDEX IX_Audit_Entity ON [Infrastructure].[AuditLogs](EntityName, EntityRef) WITH (DATA_COMPRESSION = PAGE);

GO



PRINT N'=== 1.1 Створення індексів для швидкодії... ===';

CREATE NONCLUSTERED INDEX IX_Classes_AcademicYear ON [Core].[Classes](AcademicYear) WITH (DATA_COMPRESSION = PAGE);

CREATE UNIQUE NONCLUSTERED INDEX IX_Classes_HomeroomTeacher_Active ON [Core].[Classes](HomeroomTeacherId) WHERE IsActive = 1;

CREATE UNIQUE NONCLUSTERED INDEX IX_Teachers_UserId ON [Core].[Teachers](UserId) WHERE UserId IS NOT NULL;

CREATE UNIQUE NONCLUSTERED INDEX IX_Students_UserId ON [Core].[Students](UserId) WHERE UserId IS NOT NULL;

CREATE NONCLUSTERED INDEX IX_StudentParents_ParentId ON [Core].[StudentParents](ParentId) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Students_ClassId ON [Core].[Students](ClassId) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Students_IsActive ON [Core].[Students](IsActive) INCLUDE (ClassId, LastName, FirstName) WHERE IsActive = 1 WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Lessons_AssignmentId_Date ON [Operations].[Lessons](AssignmentId, LessonDate) INCLUDE (LessonTopic, LessonTypeId, PeriodId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Lessons_Date ON [Operations].[Lessons](LessonDate) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Grades_StudentId ON [Operations].[Grades](StudentId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Grades_LessonId ON [Operations].[Grades](LessonId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Grades_CreatedByUserId ON [Operations].[Grades](CreatedByUserId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Grades_UpdatedByUserId ON [Operations].[Grades](UpdatedByUserId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_TeachingAssignments_TeacherId ON [Operations].[TeachingAssignments](TeacherId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_TeachingAssignments_ClassId ON [Operations].[TeachingAssignments](ClassId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_TeacherSubstitutions_AssignmentId ON [Operations].[TeacherSubstitutions](AssignmentId) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Lessons_SemesterId ON [Operations].[Lessons](SemesterId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Grades_GradeTypeId ON [Operations].[Grades](GradeTypeId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Attendance_Lesson_Student ON [Operations].[Attendances](LessonId, StudentId) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_QuizAssignments_QuizId ON [Operations].[QuizAssignments](QuizId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_QuizAssignments_ClassId ON [Operations].[QuizAssignments](ClassId) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_RefreshTokens_UserId_ExpiresAt ON [Identity].[RefreshTokens](UserId, ExpiresAt) INCLUDE (Revoked) WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Announcements_Active ON [Communications].[Announcements](DateCreated DESC) INCLUDE (Title, ExpirationDate) WHERE IsActive = 1 WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_OutboxMessages_OccurredOnUtc ON [Infrastructure].[OutboxMessages](OccurredOnUtc) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_OutboxMessages_Unprocessed ON [Infrastructure].[OutboxMessages](OccurredOnUtc) WHERE ProcessedOnUtc IS NULL WITH (DATA_COMPRESSION = PAGE);

CREATE NONCLUSTERED INDEX IX_Teachers_PositionId ON [Core].[Teachers](PositionId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Teachers_QualificationId ON [Core].[Teachers](QualificationId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Teachers_PedagogicalTitleId ON [Core].[Teachers](PedagogicalTitleId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_QuizQuestions_QuizId ON [Operations].[QuizQuestions](QuizId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_AuditLogs_UserId ON [Infrastructure].[AuditLogs](UserId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Subgroups_ClassId ON [Core].[Subgroups](ClassId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Subgroups_SubjectId ON [Core].[Subgroups](SubjectId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Users_RoleId ON [Identity].[Users](RoleId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_TeachingAssignments_SubjectId ON [Operations].[TeachingAssignments](SubjectId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_TeachingAssignments_SubgroupId ON [Operations].[TeachingAssignments](SubgroupId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_FixedSchedules_PeriodId ON [Operations].[FixedSchedules](PeriodId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_FixedSchedules_AssignmentId ON [Operations].[FixedSchedules](AssignmentId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_FixedSchedules_RoomId ON [Operations].[FixedSchedules](RoomId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Lessons_RoomId ON [Operations].[Lessons](RoomId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_TeacherSubstitutions_SubstituteTeacherId ON [Operations].[TeacherSubstitutions](SubstituteTeacherId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_Announcements_AuthorId ON [Communications].[Announcements](AuthorId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_OutboxMessages_CreatedByUserId ON [Infrastructure].[OutboxMessages](CreatedByUserId) WITH (DATA_COMPRESSION = PAGE);
CREATE NONCLUSTERED INDEX IX_SystemSettings_UpdatedByUserId ON [Infrastructure].[SystemSettings](UpdatedByUserId) WITH (DATA_COMPRESSION = PAGE);
GO

CREATE OR ALTER VIEW [Core].[vw_Read_StudentProfiles] AS
SELECT 
    s.StudentId,
    s.LastName,
    s.FirstName,
    s.MiddleName,
    s.DateOfBirth,
    s.Gender,
    c.ClassId,
    c.ClassName,
    t.TeacherId AS HomeroomTeacherId,
    t.LastName + ' ' + t.FirstName AS HomeroomTeacherName,
    u.Email AS StudentEmail,
    s.UserId,
    s.IsActive
FROM [Core].[Students] s
LEFT JOIN [Core].[Classes] c ON s.ClassId = c.ClassId
LEFT JOIN [Core].[Teachers] t ON c.HomeroomTeacherId = t.TeacherId
LEFT JOIN [Identity].[Users] u ON s.UserId = u.UserId;
GO

CREATE OR ALTER VIEW [Core].[vw_Read_TeacherProfiles] AS
SELECT 
    t.TeacherId,
    t.LastName,
    t.FirstName,
    t.MiddleName,
    t.Phone,
    t.Specialization,
    u.UserId,
    u.Email,
    u.Login,
    ar.RoleName,
    p.PositionName,
    q.QualificationName,
    t.IsActive
FROM [Core].[Teachers] t
LEFT JOIN [Identity].[Users] u ON t.UserId = u.UserId
LEFT JOIN [Identity].[Roles] ar ON u.RoleId = ar.RoleId
LEFT JOIN [Reference].[Positions] p ON t.PositionId = p.PositionId
LEFT JOIN [Reference].[Qualifications] q ON t.QualificationId = q.QualificationId;
GO



PRINT N'=== 2. Наповнення довідників... ===';

INSERT INTO [Reference].[Semesters] (SemesterName, StartDate, EndDate) VALUES
(N'1 семестр', '2025-09-01', '2025-12-26'),
(N'2 семестр', '2026-01-12', '2026-05-29');

INSERT INTO [Infrastructure].[SystemSettings] (SettingKey, SchoolName, AcademicYear, PrincipalName, UpdatedByUserId)
VALUES (1, N'Хрінівська філія Іллінецького ліцею №1', N'2025-2026', N'Шевченко Олег Іванович', NEWID());

INSERT INTO [Identity].[Roles] (RoleName, Description) VALUES
(N'Admin', N'Адміністратор БД. Повний доступ до системи та налаштувань'),
(N'Director', N'Адміністратор Закладу (Директор/Завуч). Перегляд та редагування всього контенту'),
(N'Teacher', N'Вчитель. Доступ до своїх предметів. Класні керівники бачать свій клас.'),
(N'Student', N'Учень. Доступ до щоденника, розкладу та оцінок'),
(N'Parent', N'Батько/Опікун. Моніторинг успішності та відвідуваності своїх дітей');

INSERT INTO [Reference].[Qualifications] (QualificationName) VALUES
(N'Вища категорія'), 
(N'Перша категорія'), 
(N'Друга категорія'), 
(N'Спеціаліст');

INSERT INTO [Reference].[PedagogicalTitles] (TitleName) VALUES
(N'Вчитель-методист'), 
(N'Старший вчитель');

INSERT INTO [Reference].[Positions] (PositionName) VALUES
(N'Вчитель'), 
(N'Директор школи'), 
(N'Заступник директора'), 
(N'Практичний психолог'), 
(N'Вчитель/Адміністратор БД');

INSERT INTO [Reference].[GradeTypes] (TypeName)
SELECT Name
FROM (VALUES 
(1, N'Поточна'), (2, N'Формувальна'), (3, N'Зошит'), (4, N'Самостійна робота'), (5, N'Практична робота'), 
(6, N'Лабораторна робота'), (7, N'Проєкт'), (8, N'Діагностувальна робота'), (9, N'Тематична'), (10, N'Група результатів 1 (ГР1)'), 
(11, N'Група результатів 2 (ГР2)'), (12, N'Група результатів 3 (ГР3)'), (13, N'Група результатів 4 (ГР4)'), (14, N'І семестр'), 
(15, N'ІІ семестр'), (16, N'Скоригована'), (17, N'Річна'), (18, N'ДПА')
) AS Data(Id, Name);

INSERT INTO [Reference].[LessonTypes] (TypeName) VALUES
(N'Засвоєння нових знань'),
(N'Формування умінь і навичок'),
(N'Застосування знань і умінь'),
(N'Узагальнення і систематизація'),
(N'Діагностування (Контроль та корекція)'),
(N'Комбінований урок'),
(N'Проєктна робота / Дослідження');

INSERT INTO [Reference].[BellSchedules] (LessonNumber, StartTime, EndTime) VALUES
(1, '08:30', '09:15'), (2, '09:25', '10:10'), (3, '10:30', '11:15'),
(4, '11:35', '12:20'), (5, '12:40', '13:25'), (6, '13:35', '14:20'), (7, '14:30', '15:15');

INSERT INTO [Reference].[Classrooms] (RoomNumber, Name, Capacity) VALUES
(N'101', N'Кабінет Математики', 30), (N'102', N'Кабінет Фізики', 30), 
(N'201', N'Кабінет Інформатики', 15), (N'202', N'Кабінет Хімії', 30), 
(N'Спортзал', N'Спортивна зала', 60);

INSERT INTO [Core].[Subjects] (SubjectName)
SELECT Name
FROM (VALUES 
(N'Українська мова'), (N'Українська література'), (N'Зарубіжна література'), (N'Англійська мова'),
(N'Математика'), (N'Алгебра'), (N'Геометрія'), (N'Інформатика'),
(N'Пізнаємо природу'), (N'Біологія'), (N'Географія'), (N'Фізика'), (N'Хімія'),    
(N'Історія України'), (N'Всесвітня історія'), (N'Правознавство'), 
(N'Музичне мистецтво'), (N'Образотворче мистецтво'), (N'Мистецтво'), 
(N'Трудове навчання'), (N'Основи здоров''я'), (N'Фізична культура')
) AS Data(Name);

INSERT INTO [Communications].[Announcements] (Title, Content, ExpirationDate, AuthorId) VALUES
(N'Зимові канікули!', N'Увага! Зимові канікули розпочинаються з 29 грудня. Бажаємо гарного відпочинку!', DATEADD(DAY, 30, GETUTCDATE()), NEWID()), 
(N'Батьківські збори', N'Шановні батьки, нагадуємо про батьківські збори 15 грудня о 18:00.', DATEADD(DAY, 14, GETUTCDATE()), NEWID());



PRINT N'=== 4. Персонал... ===';

DECLARE @Vyscha UNIQUEIDENTIFIER = (SELECT QualificationId FROM [Reference].[Qualifications] WHERE QualificationName = N'Вища категорія'); 
DECLARE @Persha UNIQUEIDENTIFIER = (SELECT QualificationId FROM [Reference].[Qualifications] WHERE QualificationName = N'Перша категорія'); 
DECLARE @Druha UNIQUEIDENTIFIER = (SELECT QualificationId FROM [Reference].[Qualifications] WHERE QualificationName = N'Друга категорія'); 
DECLARE @Specialist UNIQUEIDENTIFIER = (SELECT QualificationId FROM [Reference].[Qualifications] WHERE QualificationName = N'Спеціаліст');
DECLARE @Metodyst UNIQUEIDENTIFIER = (SELECT TitleId FROM [Reference].[PedagogicalTitles] WHERE TitleName = N'Вчитель-методист'); 
DECLARE @Starshyy UNIQUEIDENTIFIER = (SELECT TitleId FROM [Reference].[PedagogicalTitles] WHERE TitleName = N'Старший вчитель');
DECLARE @Vchytel UNIQUEIDENTIFIER = (SELECT PositionId FROM [Reference].[Positions] WHERE PositionName = N'Вчитель'); 
DECLARE @Director UNIQUEIDENTIFIER = (SELECT PositionId FROM [Reference].[Positions] WHERE PositionName = N'Директор школи'); 
DECLARE @Zastupnyk UNIQUEIDENTIFIER = (SELECT PositionId FROM [Reference].[Positions] WHERE PositionName = N'Заступник директора'); 
DECLARE @Psyholog UNIQUEIDENTIFIER = (SELECT PositionId FROM [Reference].[Positions] WHERE PositionName = N'Практичний психолог'); 
DECLARE @Vch_Admin UNIQUEIDENTIFIER = (SELECT PositionId FROM [Reference].[Positions] WHERE PositionName = N'Вчитель/Адміністратор БД');

INSERT INTO [Core].[Teachers] (LastName, FirstName, MIddleName, Phone, Specialization, DateOfBirth, Gender, Workload, EducationInfo, PositionId, QualificationId, PedagogicalTitleId, UserId)
SELECT LN, FN, MN, Ph, Sp, DoB, G, W, EI, PId, QId, PTId, NULL
FROM (VALUES
(N'Коваленко', N'Ірина', N'Петрівна', '(097) 111-22-33', N'Математика', '1975-03-12', 'Female', 1.25, N'Вінницький ДПУ', @Vchytel, @Vyscha, @Metodyst), 
(N'Шевченко', N'Олег', N'Іванович', '(093) 222-33-44', N'Фізкультура/Основи здоров''я', '1988-07-20', 'Male', 1.25, N'НУФВСУ', @Director, @Persha, NULL),
(N'Слово', N'Галина', N'Михайлівна', '(095) 123-32-11', N'Українська мова/літ', '1970-05-05', 'Female', 1.5, N'НПУ Драгомаманова', @Vchytel, @Vyscha, @Metodyst),
(N'Петренко', N'Ігор', N'Васильович', '(050) 111-22-00', N'Історія/Правознавство', '1979-09-19', 'Male', 0.75, N'ХНУВС', @Vchytel, @Vyscha, @Starshyy),
(N'Садовий', N'Михайло', N'Петрович', '(067) 555-66-77', N'Біологія/Географія', '1985-07-07', 'Male', 0.75, N'УДПУ', @Vchytel, @Persha, NULL),
(N'Блек', N'Джессіка', N'Дмитрівна', '(093) 444-55-66', N'Англійська мова', '1995-02-14', 'Female', 0.50, N'ЛНУ Франка', @Vchytel, @Specialist, NULL),
(N'Ньютон', N'Василь', N'Ісаакович', '(050) 333-22-11', N'Фізика/Інформатика/Адміністратор БД', '1988-12-12', 'Male', 1.25, N'КНУ Фізфак/КПІ', @Vch_Admin, @Persha, NULL),
(N'Глухота', N'Марія', N'Анатоліївна', '(093) 123-45-67', N'Мистецтво/Технології', '1985-05-20', 'Female', 1.25, N'Вінницький ДПУ', @Zastupnyk, @Persha, NULL),
(N'Васильчук', N'Оксана', N'Дмитрівна', '(050) 345-67-89', N'Психолог', '1991-12-05', 'Female', 1.00, N'Університет Грінченка', @Psyholog, @Druha, NULL),
(N'Ткаченко', N'Олена', N'Василівна', '(098) 555-66-77', N'Хімія/Математика/Заруб. літ.', '1990-02-28', 'Female', 1.00, N'ЖДУ Франка', @Vchytel, @Druha, NULL)
) AS Data(LN, FN, MN, Ph, Sp, DoB, G, W, EI, PId, QId, PTId);

GO


PRINT N'=== 3. Налаштування АДМІНА... ===';

DECLARE @AdminUserId UNIQUEIDENTIFIER = NEWId();

INSERT INTO [Identity].[Users] (UserId, Login, Email, PasswordHash, RoleId)
VALUES (@AdminUserId, 'admin', 'admin@school.ua', 'BgpoOZJKmgihrXwGiKCovg==:gdHBs4uwY7c+6QNIuc/yfiICHPcJ7frcwO4zRDoBnXk=', (SELECT RoleId FROM [Identity].[Roles] WHERE RoleName = 'Admin'));

UPDATE [Core].[Teachers] SET UserId = @AdminUserId WHERE LastName = N'Ньютон' AND FirstName = N'Василь';

DECLARE @DirectorUserId UNIQUEIDENTIFIER = NEWId();
DECLARE @DirectorRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM [Identity].[Roles] WHERE RoleName = N'Director');

INSERT INTO [Identity].[Users] (UserId, Login, Email, PasswordHash, RoleId) 
VALUES (@DirectorUserId, 'director', 'director@school.ua', 'BgpoOZJKmgihrXwGiKCovg==:gdHBs4uwY7c+6QNIuc/yfiICHPcJ7frcwO4zRDoBnXk=', @DirectorRoleId);

UPDATE [Core].[Teachers] SET UserId = @DirectorUserId WHERE LastName = N'Шевченко' AND FirstName = N'Олег';

DECLARE @TeacherUserId UNIQUEIDENTIFIER = NEWId();
DECLARE @TeacherRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM [Identity].[Roles] WHERE RoleName = N'Teacher');

INSERT INTO [Identity].[Users] (UserId, Login, Email, PasswordHash, RoleId) 
VALUES (@TeacherUserId, 'teacher', 'teacher@school.ua', 'BgpoOZJKmgihrXwGiKCovg==:gdHBs4uwY7c+6QNIuc/yfiICHPcJ7frcwO4zRDoBnXk=', @TeacherRoleId);

UPDATE [Core].[Teachers] SET UserId = @TeacherUserId WHERE LastName = N'Коваленко' AND FirstName = N'Ірина';

DECLARE @StudentUserId UNIQUEIDENTIFIER = NEWId();
DECLARE @StudentRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM [Identity].[Roles] WHERE RoleName = N'Student');

INSERT INTO [Identity].[Users] (UserId, Login, Email, PasswordHash, RoleId) 
VALUES (@StudentUserId, 'student', 'student@school.ua', 'BgpoOZJKmgihrXwGiKCovg==:gdHBs4uwY7c+6QNIuc/yfiICHPcJ7frcwO4zRDoBnXk=', @StudentRoleId);

UPDATE [Core].[Students] SET UserId = @StudentUserId WHERE LastName = N'Коваленко' AND FirstName = N'Олександр';

GO



PRINT N'=== 5. Класи... ===';

INSERT INTO [Core].[Classes] (ClassName, GradeLevel, AcademicYear, HomeroomTeacherId)
SELECT Data.Name, Data.Gr, Data.AY, t.TeacherId
FROM (VALUES 
(N'5', 5, N'2025-2026', N'Петренко', N'Ігор'), 
(N'6', 6, N'2025-2026', N'Слово', N'Галина'), 
(N'7', 7, N'2025-2026', N'Садовий', N'Михайло'), 
(N'8', 8, N'2025-2026', N'Блек', N'Джессіка'), 
(N'9', 9, N'2025-2026', N'Коваленко', N'Ірина')
) AS Data(Name, Gr, AY, TLN, TFN)
JOIN [Core].[Teachers] t ON t.LastName = Data.TLN AND t.FirstName = Data.TFN;

PRINT N'=== 6. Генерація УЧНІВ (З батьком і матір''ю)... ===';

IF OBJECT_Id('tempdb..#Surnames') IS NOT NULL DROP TABLE #Surnames;
CREATE TABLE #Surnames (S NVARCHAR(50));
INSERT INTO #Surnames VALUES (N'Коваленко'), (N'Бондаренко'), (N'Ткаченко'), (N'Мельник'), (N'Шевченко'), (N'Бойко'), (N'Кравченко'), (N'Козак'), (N'Олійник'), (N'Лисенко'), (N'Гаврилюк'), (N'Поліщук'), (N'Іваненко'), (N'Мороз'), (N'Петренко'), (N'Павленко'), (N'Василенко'), (N'Сидоренко'), (N'Савченко'), (N'Кузьменко'),(N'Зінченко'), (N'Марченко'), (N'Демченко'), (N'Романенко'), (N'Литвин'), (N'Бабич'), (N'Гнатюк'), (N'Волошин'), (N'Даниленко'), (N'Терещенко');

IF OBJECT_Id('tempdb..#MaleNames') IS NOT NULL DROP TABLE #MaleNames;
CREATE TABLE #MaleNames (N NVARCHAR(50));
INSERT INTO #MaleNames VALUES (N'Олександр'), (N'Максим'), (N'Дмитро'), (N'Артем'), (N'Іван'), (N'Михайло'), (N'Богдан'), (N'Андрій'), (N'Єгор'), (N'Владислав'),(N'Назар'), (N'Данило'), (N'Роман'), (N'Володимир'), (N'Тимофій'), (N'Матвій'), (N'Сергій'), (N'Ярослав'), (N'Денис'), (N'Олексій');

IF OBJECT_Id('tempdb..#FemaleNames') IS NOT NULL DROP TABLE #FemaleNames;
CREATE TABLE #FemaleNames (N NVARCHAR(50));
INSERT INTO #FemaleNames VALUES (N'Анна'), (N'Софія'), (N'Марія'), (N'Вікторія'), (N'Дарина'), (N'Анастасія'), (N'Поліна'), (N'Вероніка'), (N'Єва'), (N'Злата'),(N'Мілана'), (N'Соломія'), (N'Олександра'), (N'Ольга'), (N'Юлія'), (N'Тетяна'), (N'Яна'), (N'Діана'), (N'Катерина'), (N'Аліса');

DECLARE class_cursor CURSOR FOR SELECT ClassId, GradeLevel FROM [Core].[Classes];
OPEN class_cursor;
DECLARE @ClassId UNIQUEIDENTIFIER, @GradeLevel INT;
FETCH NEXT FROM class_cursor INTO @ClassId, @GradeLevel;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @StudentCount INT = 1;
    DECLARE @TotalStudents INT = 18 + (ABS(CHECKSUM(NEWID())) % 8);

    WHILE @StudentCount <= @TotalStudents
    BEGIN
        DECLARE @IsMale BIT = CAST(ABS(CHECKSUM(NEWId())) % 2 AS BIT);
        DECLARE @Gen NVARCHAR(10);
        DECLARE @FirstName NVARCHAR(50); 
        DECLARE @Surname NVARCHAR(50);
        
        SELECT TOP 1 @Surname = S FROM #Surnames ORDER BY NEWId();

    IF @IsMale = 1 
        BEGIN 
            SET @Gen = 'Male'; 
            SELECT TOP 1 @FirstName = N FROM #MaleNames ORDER BY NEWId(); 
        END
        ELSE 
        BEGIN 
            SET @Gen = 'Female'; 
            SELECT TOP 1 @FirstName = N FROM #FemaleNames ORDER BY NEWId(); 
            IF RIGHT(@Surname, 2) IN (N'ов', N'єв', N'ін') SET @Surname = @Surname + N'а';
            IF RIGHT(@Surname, 2) = N'ий' SET @Surname = LEFT(@Surname, LEN(@Surname)-2) + N'а'; 
        END

        DECLARE @DadNameBase NVARCHAR(50); 
        SELECT TOP 1 @DadNameBase = N FROM #MaleNames ORDER BY NEWId();
        
DECLARE @MiddleName NVARCHAR(50);
        SET @MiddleName = @DadNameBase + CASE WHEN @IsMale=1 THEN N'ович' ELSE N'івна' END;
        
        DECLARE @YearOfBirth INT = 2025 - 10 - (@GradeLevel - 5);
        DECLARE @DOB DATE = DATEFROMPARTS(@YearOfBirth, (ABS(CHECKSUM(NEWID())) % 12) + 1, (ABS(CHECKSUM(NEWID())) % 28) + 1);

DECLARE @InsertedStudent TABLE (Id UNIQUEIDENTIFIER); DELETE FROM @InsertedStudent;
        DECLARE @InsertedDad TABLE (Id UNIQUEIDENTIFIER); DELETE FROM @InsertedDad;
        DECLARE @InsertedMom TABLE (Id UNIQUEIDENTIFIER); DELETE FROM @InsertedMom;

        INSERT INTO [Core].[Students] (LastName, FirstName, MiddleName, DateOfBirth, ClassId, Gender, DocumentType, DocumentSeries, DocumentNumber, EnrollmentDate, EnrollmentReason, Address, MedicalNotes)
        OUTPUT inserted.StudentId INTO @InsertedStudent
        VALUES (@Surname, @FirstName, @MiddleName, @DOB, @ClassId, @Gen, N'Свідоцтво', N'I-AM', CAST(100000+ABS(CHECKSUM(NEWID()))%900000 AS NVARCHAR), '2021-09-01', N'Заява', N'вул. Шкільна, ' + CAST(ABS(CHECKSUM(NEWID())) % 100 AS NVARCHAR), NULL);

        DECLARE @SId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM @InsertedStudent);

        INSERT INTO [Core].[Parents] (LastName, FirstName, MIddleName, Phone) 
        OUTPUT inserted.ParentId INTO @InsertedDad
        SELECT @Surname, @DadNameBase, N'Іванович', N'+38050' + CAST(1000000 + ABS(CHECKSUM(NEWID())) % 8999999 AS NVARCHAR);
        
        DECLARE @DadId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM @InsertedDad);
        INSERT INTO [Core].[StudentParents] (StudentId, ParentId, Role) VALUES (@SId, @DadId, N'Батько');
            
        DECLARE @MomName NVARCHAR(50); SELECT TOP 1 @MomName = N FROM #FemaleNames ORDER BY NEWId();
        DECLARE @MomSurname NVARCHAR(50) = @Surname; 
        IF @IsMale = 1
        BEGIN
             IF RIGHT(@MomSurname, 2) IN (N'ов', N'єв', N'ін') SET @MomSurname = @MomSurname + N'а';
             IF RIGHT(@MomSurname, 2) = N'ий' SET @MomSurname = LEFT(@MomSurname, LEN(@MomSurname)-2) + N'а';
        END

        INSERT INTO [Core].[Parents] (LastName, FirstName, MIddleName, Phone) 
        OUTPUT inserted.ParentId INTO @InsertedMom
        SELECT @MomSurname, @MomName, N'Петрівна', N'+38067' + CAST(1000000 + ABS(CHECKSUM(NEWID())) % 8999999 AS NVARCHAR);
        
        DECLARE @MomId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM @InsertedMom);
        INSERT INTO [Core].[StudentParents] (StudentId, ParentId, Role) VALUES (@SId, @MomId, N'Мати');
        
       SET @StudentCount = @StudentCount + 1;
    END
    FETCH NEXT FROM class_cursor INTO @ClassId, @GradeLevel;
END
CLOSE class_cursor; DEALLOCATE class_cursor;
DROP TABLE #Surnames; DROP TABLE #MaleNames; DROP TABLE #FemaleNames;
GO



PRINT N'=== 7. Навантаження... ===';

DECLARE @C5 UNIQUEIDENTIFIER = (SELECT ClassId FROM [Core].[Classes] WHERE ClassName = N'5'); 
DECLARE @C6 UNIQUEIDENTIFIER = (SELECT ClassId FROM [Core].[Classes] WHERE ClassName = N'6'); 
DECLARE @C7 UNIQUEIDENTIFIER = (SELECT ClassId FROM [Core].[Classes] WHERE ClassName = N'7'); 
DECLARE @C8 UNIQUEIDENTIFIER = (SELECT ClassId FROM [Core].[Classes] WHERE ClassName = N'8'); 
DECLARE @C9 UNIQUEIDENTIFIER = (SELECT ClassId FROM [Core].[Classes] WHERE ClassName = N'9');

INSERT INTO [Operations].[TeachingAssignments] (TeacherId, SubjectId, ClassId) 
SELECT t.TeacherId, s.SubjectId, Data.CId
FROM (VALUES 
(N'Ткаченко', N'Математика', @C5), (N'Шевченко', N'Фізична культура', @C5), (N'Слово', N'Українська мова', @C5), (N'Слово', N'Українська література', @C5), (N'Блек', N'Англійська мова', @C5), (N'Петренко', N'Історія України', @C5), (N'Садовий', N'Пізнаємо природу', @C5), (N'Ньютон', N'Інформатика', @C5), (N'Глухота', N'Музичне мистецтво', @C5), (N'Глухота', N'Образотворче мистецтво', @C5), (N'Глухота', N'Мистецтво', @C5), (N'Глухота', N'Трудове навчання', @C5), (N'Шевченко', N'Основи здоров''я', @C5), (N'Ткаченко', N'Зарубіжна література', @C5),
(N'Ткаченко', N'Математика', @C6), (N'Слово', N'Українська мова', @C6), (N'Слово', N'Українська література', @C6), (N'Блек', N'Англійська мова', @C6), (N'Петренко', N'Історія України', @C6), (N'Петренко', N'Всесвітня історія', @C6), (N'Садовий', N'Пізнаємо природу', @C6), (N'Садовий', N'Географія', @C6), (N'Ньютон', N'Інформатика', @C6), (N'Глухота', N'Трудове навчання', @C6), (N'Глухота', N'Музичне мистецтво', @C6), (N'Шевченко', N'Основи здоров''я', @C6), (N'Шевченко', N'Фізична культура', @C6), (N'Ткаченко', N'Зарубіжна література', @C6),
(N'Коваленко', N'Алгебра', @C7), (N'Коваленко', N'Геометрія', @C7), (N'Ткаченко', N'Хімія', @C7), (N'Слово', N'Українська мова', @C7), (N'Слово', N'Українська література', @C7), (N'Блек', N'Англійська мова', @C7), (N'Ньютон', N'Фізика', @C7), (N'Садовий', N'Біологія', @C7), (N'Садовий', N'Географія', @C7), (N'Петренко', N'Історія України', @C7), (N'Петренко', N'Всесвітня історія', @C7), (N'Ньютон', N'Інформатика', @C7), (N'Глухота', N'Трудове навчання', @C7), (N'Шевченко', N'Основи здоров''я', @C7), (N'Шевченко', N'Фізична культура', @C7), (N'Ткаченко', N'Зарубіжна література', @C7),
(N'Коваленко', N'Алгебра', @C8), (N'Коваленко', N'Геометрія', @C8), (N'Ткаченко', N'Хімія', @C8), (N'Слово', N'Українська мова', @C8), (N'Слово', N'Українська література', @C8), (N'Блек', N'Англійська мова', @C8), (N'Ньютон', N'Фізика', @C8), (N'Садовий', N'Біологія', @C8), (N'Садовий', N'Географія', @C8), (N'Петренко', N'Історія України', @C8), (N'Петренко', N'Всесвітня історія', @C8), (N'Ньютон', N'Інформатика', @C8), (N'Глухота', N'Мистецтво', @C8), (N'Глухота', N'Трудове навчання', @C8), (N'Шевченко', N'Основи здоров''я', @C8), (N'Шевченко', N'Фізична культура', @C8), (N'Ткаченко', N'Зарубіжна література', @C8),
(N'Коваленко', N'Алгебра', @C9), (N'Коваленко', N'Геометрія', @C9), (N'Ткаченко', N'Хімія', @C9), (N'Слово', N'Українська мова', @C9), (N'Слово', N'Українська література', @C9), (N'Блек', N'Англійська мова', @C9), (N'Ньютон', N'Фізика', @C9), (N'Садовий', N'Біологія', @C9), (N'Садовий', N'Географія', @C9), (N'Петренко', N'Історія України', @C9), (N'Петренко', N'Всесвітня історія', @C9), (N'Петренко', N'Правознавство', @C9), (N'Ньютон', N'Інформатика', @C9), (N'Глухота', N'Мистецтво', @C9), (N'Глухота', N'Трудове навчання', @C9), (N'Шевченко', N'Основи здоров''я', @C9), (N'Шевченко', N'Фізична культура', @C9), (N'Ткаченко', N'Зарубіжна література', @C9)
) AS Data(TLN, SN, CId)
JOIN [Core].[Teachers] t ON t.LastName = Data.TLN
JOIN [Core].[Subjects] s ON s.SubjectName = Data.SN;

GO



PRINT N'=== 8. Генерація фіксованого розкладу... ===';

DECLARE schedule_cursor CURSOR FOR SELECT ClassId, GradeLevel FROM [Core].[Classes];
OPEN schedule_cursor;
DECLARE @S_ClassId_I UNIQUEIDENTIFIER, @S_GradeLevel_I INT;
FETCH NEXT FROM schedule_cursor INTO @S_ClassId_I, @S_GradeLevel_I;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @SubjectPool_I TABLE (AssignmentId UNIQUEIDENTIFIER, Freq INT);
    DELETE FROM @SubjectPool_I;

INSERT INTO @SubjectPool_I (AssignmentId, Freq)
SELECT ta.AssignmentId, CASE WHEN s.SubjectName IN (N'Українська мова', N'Алгебра', N'Геометрія') THEN 4 WHEN s.SubjectName IN (N'Українська література', N'Англійська мова', N'Фізика', N'Історія України', N'Хімія', N'Математика', N'Біологія', N'Географія', N'Зарубіжна література') THEN 2 ELSE 1 END
FROM [Operations].[TeachingAssignments] ta JOIN [Core].[Subjects] s ON ta.SubjectId = s.SubjectId WHERE ta.ClassId = @S_ClassId_I;

DECLARE @Day_I INT = 1; DECLARE @LessonNum_I INT = 1;
    DECLARE @CurrentAssignment_I UNIQUEIDENTIFIER;
    
    WHILE EXISTS (SELECT 1 FROM @SubjectPool_I WHERE Freq > 0)
    BEGIN
    SELECT TOP 1 @CurrentAssignment_I = AssignmentId FROM @SubjectPool_I WHERE Freq > 0 ORDER BY Freq DESC, NEWID();
        
        DECLARE @RandomRoomId UNIQUEIDENTIFIER;
        SELECT TOP 1 @RandomRoomId = RoomId FROM [Reference].[Classrooms] ORDER BY NEWID();

DECLARE @PeriodId_I UNIQUEIDENTIFIER;
        SELECT @PeriodId_I = ScheduleId FROM [Reference].[BellSchedules] WHERE LessonNumber = @LessonNum_I;
        INSERT INTO [Operations].[FixedSchedules] (DayOfWeek, PeriodId, AssignmentId, RoomId) VALUES (@Day_I, @PeriodId_I, @CurrentAssignment_I, @RandomRoomId);
        UPDATE @SubjectPool_I SET Freq = Freq - 1 WHERE AssignmentId = @CurrentAssignment_I;
        
        SET @LessonNum_I = @LessonNum_I + 1;
        DECLARE @Limit_I INT = CASE WHEN @S_GradeLevel_I < 7 THEN 6 WHEN @S_GradeLevel_I = 7 THEN 6 ELSE 7 END;
        IF @LessonNum_I > @Limit_I BEGIN SET @LessonNum_I = 1; SET @Day_I = @Day_I + 1; IF @Day_I > 5 SET @Day_I = 1; END
    END
    FETCH NEXT FROM schedule_cursor INTO @S_ClassId_I, @S_GradeLevel_I;
END
CLOSE schedule_cursor; DEALLOCATE schedule_cursor;
GO

PRINT N'=== 9. Генерація Журналу та Оцінок ===';

BEGIN TRAN;

DECLARE @LT_Normal UNIQUEIDENTIFIER = (SELECT TOP 1 LessonTypeId FROM [Reference].[LessonTypes] WHERE TypeName = N'Комбінований урок');
DECLARE @LT_Assessment UNIQUEIDENTIFIER = (SELECT TOP 1 LessonTypeId FROM [Reference].[LessonTypes] WHERE TypeName = N'Діагностування (Контроль та корекція)');
DECLARE @AdminUserId UNIQUEIDENTIFIER;
SELECT TOP 1 @AdminUserId = UserId FROM [Identity].[Users] WHERE Login = 'admin';

DECLARE @NushDefinitions TABLE (
    SubjectNames NVARCHAR(500), 
    GR_Num INT,                 
    TopicText NVARCHAR(255)     
);

INSERT INTO @NushDefinitions VALUES 
(N'Математика,Алгебра,Геометрія', 1, N'Досліджує ситуації та створює математичні моделі'),
(N'Математика,Алгебра,Геометрія', 2, N'Розв’язує математичні задачі'),
(N'Математика,Алгебра,Геометрія', 3, N'Інтерпретує та критично аналізує результати'),
(N'Музичне мистецтво,Образотворче мистецтво,Мистецтво', 1, N'Пізнання мистецтва, художнє мислення'),
(N'Музичне мистецтво,Образотворче мистецтво,Мистецтво', 2, N'Художньо-творча діяльність, мистецька комунікація'),
(N'Музичне мистецтво,Образотворче мистецтво,Мистецтво', 3, N'Емоційний досвід, художньо-естетичне ставлення'),
(N'Англійська мова', 1, N'Сприймає усну інформацію на слух / Аудіювання'),
(N'Англійська мова', 2, N'Усно взаємодіє та висловлюється / Говоріння'),
(N'Англійська мова', 3, N'Сприймає письмові тексти / Читання'),
(N'Англійська мова', 4, N'Письмово взаємодіє та висловлюється / Письмо'),
(N'Трудове навчання', 1, N'Проєктує та виготовляє вироби'),
(N'Трудове навчання', 2, N'Застосовує технології декоративно-ужиткового мистецтва'),
(N'Трудове навчання', 3, N'Ефективне використання техніки і матеріалів'),
(N'Трудове навчання', 4, N'Виявляє самозарадність у побуті/освітньому процесі'),
(N'Історія України,Всесвітня історія,Правознавство', 1, N'Орієнтується в історичному часі та просторі'),
(N'Історія України,Всесвітня історія,Правознавство', 2, N'Працює з інформацією історичного змісту'),
(N'Історія України,Всесвітня історія,Правознавство', 3, N'Виявляє здатність до співпраці, громадянську позицію'),
(N'Основи здоров''я', 1, N'Безпека. Уникання загроз для життя'),
(N'Основи здоров''я', 2, N'Здоров’я. Турбота про особисте здоров’я'),
(N'Основи здоров''я', 3, N'Добробут. Підприємливість та етична поведінка'),
(N'Фізична культура', 1, N'Розвиває особистісні якості в процесі фіз. виховання'),
(N'Фізична культура', 2, N'Володіє технікою фізичних вправ'),
(N'Фізична культура', 3, N'Здійснює фізкультурно-оздоровчу діяльність'),
(N'Інформатика', 1, N'Працює з інформацією, даними, моделями'),
(N'Інформатика', 2, N'Створює інформаційні продукти'),
(N'Інформатика', 3, N'Працює в цифровому середовищі'),
(N'Інформатика', 4, N'Безпечно та відповідально працює з технологіями'),
(N'Пізнаємо природу,Біологія,Географія,Фізика,Хімія', 1, N'Досліджує природу'),
(N'Пізнаємо природу,Біологія,Географія,Фізика,Хімія', 2, N'Здійснює пошук та опрацьовує інформацію'),
(N'Пізнаємо природу,Біологія,Географія,Фізика,Хімія', 3, N'Усвідомлює закономірності природи'),
(N'Українська мова,Українська література,Зарубіжна література', 1, N'Усно взаємодіє'),
(N'Українська мова,Українська література,Зарубіжна література', 2, N'Працює з текстом'),
(N'Українська мова,Українська література,Зарубіжна література', 3, N'Письмово взаємодіє'),
(N'Українська мова,Українська література,Зарубіжна література', 4, N'Досліджує мовлення');

IF OBJECT_Id('tempdb..#Topics') IS NOT NULL DROP TABLE #Topics;
CREATE TABLE #Topics (SubjectId UNIQUEIDENTIFIER, TopicList NVARCHAR(MAX));

INSERT INTO #Topics (SubjectId, TopicList)
SELECT s.SubjectId, Data.TL
FROM (VALUES 
(N'Математика', N'Натуральні числа;Дії з натуральними числами;Рівняння;Кути та їх міра;Трикутники;Площа прямокутника;Дроби;Десяткові дроби;Відсотки;Середнє арифметичне'), 
(N'Алгебра', N'Раціональні вирази;Степінь з цілим показником;Функції;Квадратні корені;Квадратні рівняння;Системи рівнянь;Числові послідовності;Нерівності'), 
(N'Геометрія', N'Найпростіші геометричні фігури;Трикутники;Паралельні прямі;Коло і круг;Геометричні побудови;Чотирикутники;Подібність фігур;Вектори;Координати на площині'), 
(N'Українська мова', N'Вступ;Лексикологія;Фразеологія;Будова слова;Словотвір;Іменник;Прикметник;Числівник;Займенник;Дієслово;Прислівник;Синтаксис'), 
(N'Українська література', N'Усна народна творчість;Давня література;Творчість Т.Шевченка;Література ХХ ст.;Сучасна література;Поезія;Проза;Драматургія'), 
(N'Англійська мова', N'My Family;My School;My Friends;HolIdays;Travelling;Food and Drinks;London;Ukraine;Seasons and Weather;Sport;Music;Books'), 
(N'Інформатика', N'Інформація та повідомлення;Комп''ютерні пристрої;ОС Windows;Текстовий редактор;Графічний редактор;Презентації;Інтернет;Алгоритми;Програмування'), 
(N'Фізика', N'Фізичні тіла;Будова речовини;Механічний рух;Сили в природі;Тиск;Робота і енергія;Теплові явища;Електричний струм;Світлові явища'), 
(N'Хімія', N'Початкові хімічні поняття;Кисень;Вода;Розчини;Основні класи неорганічних сполук;Періодичний закон;Хімічний зв''язок'), 
(N'Географія', N'Земля у Всесвіті;План місцевості;Географічна карта;Літосфера;Атмосфера;Гідросфера;Біосфера;Населення Землі;Країни світу'), 
(N'Історія України', N'Вступ до історії;Київська Русь;Козацька доба;Українські землі у складі імперій;Українська революція;Друга світова війна;Незалежна Україна'), 
(N'Біологія', N'Клітина;Рослини;Гриби;Бактерії;Тварини;Людина;Розмноження;Спадковість;Еволюція;Екологія'), 
(N'Фізична культура', N'Легка атлетика;Гімнастика;Волейбол;Баскетбол;Футбол;Рухливі ігри'), 
(N'Музичне мистецтво', N'Музичне мистецтво;Народна музика;Класична музика;Сучасна музика;Джаз;Рок;Поп-музика;Музичні інструменти'),
(N'Трудове навчання', N'Конструювання;Моделювання;Технологічні процеси;Обробка деревини;Обробка металів;Дизайн;Декоративно-ужиткове мистецтво'),
(N'Зарубіжна література', N'Вступ;Ренесанс;Бароко;Класицизм;Романтизм;Реалізм;Модернізм;Постмодернізм;Літературні жанри;Теорія літератури')
) AS Data(SN, TL)
JOIN [Core].[Subjects] s ON s.SubjectName = Data.SN;

DECLARE @SubstAssId UNIQUEIDENTIFIER = (SELECT TOP 1 AssignmentId FROM [Operations].[TeachingAssignments]);
IF @SubstAssId IS NOT NULL
BEGIN
INSERT INTO [Operations].[TeacherSubstitutions] (AssignmentId, SubstituteTeacherId, StartDate, EndDate)
    SELECT @SubstAssId, TeacherId, GETUTCDATE(), DATEADD(day, 7, GETUTCDATE())
    FROM [Core].[Teachers] WHERE LastName = N'Петренко' AND FirstName = N'Ігор';
END

    DECLARE @StartDate DATE;
    
    SELECT @StartDate = MIN(StartDate) FROM [Reference].[Semesters];
DECLARE @EndDate DATE;
SELECT @EndDate = MAX(EndDate) FROM [Reference].[Semesters];
DECLARE @TodaySimulated DATE = CAST(GETUTCDATE() AS DATE);

DECLARE @ActiveSickness TABLE (StudentId UNIQUEIDENTIFIER, SickUntil DATE);
DECLARE @CurrentDate DATE = @StartDate;

WHILE @CurrentDate <= @EndDate
BEGIN
DECLARE @CurrentSemId UNIQUEIDENTIFIER;
    SELECT @CurrentSemId = SemesterId FROM [Reference].[Semesters] WHERE @CurrentDate BETWEEN StartDate AND EndDate;
    
    IF @CurrentSemId IS NOT NULL
    BEGIN
        DECLARE @DOW_L INT = DATEPART(WEEKDAY, @CurrentDate);

        IF @DOW_L BETWEEN 2 AND 6
        BEGIN
            DECLARE lessons_cursor_L CURSOR FOR
            SELECT fs.AssignmentId, s.SubjectName, s.SubjectId, ta.ClassId, fs.PeriodId 
            FROM [Operations].[FixedSchedules] fs
            JOIN [Operations].[TeachingAssignments] ta ON fs.AssignmentId = ta.AssignmentId
            JOIN [Core].[Subjects] s ON ta.SubjectId = s.SubjectId
            JOIN [Reference].[BellSchedules] bs ON fs.PeriodId = bs.ScheduleId
            WHERE fs.DayOfWeek = @DOW_L
            ORDER BY bs.LessonNumber;

            OPEN lessons_cursor_L;
            DECLARE @L_AssId UNIQUEIDENTIFIER, @L_SubjName NVARCHAR(100), @L_SubjId UNIQUEIDENTIFIER, @L_ClassId UNIQUEIDENTIFIER, @L_Period UNIQUEIDENTIFIER; 
            FETCH NEXT FROM lessons_cursor_L INTO @L_AssId, @L_SubjName, @L_SubjId, @L_ClassId, @L_Period;

WHILE @@FETCH_STATUS = 0
            BEGIN
                DECLARE @Cnt INT; 
                SELECT @Cnt = COUNT(*) FROM [Operations].[Lessons] WHERE AssignmentId = @L_AssId;
                DECLARE @LessonNum INT = @Cnt + 1;

                DECLARE @LessonTypeId UNIQUEIDENTIFIER; 
                DECLARE @GradeType UNIQUEIDENTIFIER;   
                DECLARE @LessonTopicName NVARCHAR(255);
                DECLARE @HomeworkText NVARCHAR(255) = N'Опрацювати матеріал';
                
                DECLARE @BaseTopic NVARCHAR(255);
                DECLARE @TopicString NVARCHAR(MAX);
                SELECT @TopicString = TopicList FROM #Topics WHERE SubjectId = @L_SubjId;
                
                IF @TopicString IS NOT NULL
                BEGIN
                    DECLARE @XML XML = CAST('<t>' + REPLACE(@TopicString, ';', '</t><t>') + '</t>' AS XML);
                    DECLARE @TCount INT = @XML.value('count(/t)', 'int');
                    DECLARE @TIndex INT = (@Cnt % @TCount) + 1;
                    
                    WITH T AS (SELECT ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS Num, n.value('.', 'NVARCHAR(255)') AS Topic FROM @XML.nodes('/t') AS T(n))
                    SELECT @BaseTopic = Topic FROM T WHERE Num = @TIndex;
                END
                ELSE
                BEGIN
                    SET @BaseTopic = N'Вивчення нової теми';
                END

                IF @LessonNum % 10 = 0
                BEGIN
                    SET @LessonTypeId = @LT_Assessment; 
                    SET @GradeType    = (SELECT TOP 1 GradeTypeId FROM [Reference].[GradeTypes] WHERE TypeName = N'Тематична'); 
                    SET @LessonTopicName = N'Підсумкова тематична робота (' + @BaseTopic + N')';
                END
                ELSE
                BEGIN
                    SET @LessonTypeId = @LT_Normal;
                    DECLARE @MaxGRs INT = 0;
                SELECT @MaxGRs = MAX(GR_Num) 
                    FROM @NushDefinitions 
                    WHERE ',' + SubjectNames + ',' LIKE N'%,' + @L_SubjName + N',%';
                    IF @MaxGRs > 0
                    BEGIN
                        DECLARE @CycleNum INT = ((@LessonNum - 1) % @MaxGRs) + 1;
                        DECLARE @GR_Description NVARCHAR(255);
                        
                    SELECT @GR_Description = TopicText
                        FROM @NushDefinitions
                        WHERE GR_Num = @CycleNum 
                          AND ',' + SubjectNames + ',' LIKE N'%,' + @L_SubjName + N',%';

                        SET @GradeType = (SELECT TOP 1 GradeTypeId FROM [Reference].[GradeTypes] WHERE TypeName LIKE N'Група результатів ' + CAST(@CycleNum AS NVARCHAR(1)) + '%');
                        SET @LessonTopicName = @BaseTopic + N' (' + @GR_Description + N')';
                    END
                ELSE
                    BEGIN
                        SET @GradeType = (SELECT TOP 1 GradeTypeId FROM [Reference].[GradeTypes] WHERE TypeName = N'Поточна'); 
                        SET @LessonTopicName = @BaseTopic;
                    END
                END

                DECLARE @InsertedLesson TABLE (Id UNIQUEIDENTIFIER); 
                DELETE FROM @InsertedLesson;
                
                DECLARE @RandomRoomId UNIQUEIDENTIFIER;
                SELECT TOP 1 @RandomRoomId = RoomId FROM [Reference].[Classrooms] ORDER BY NEWID();

                INSERT INTO [Operations].[Lessons] (AssignmentId, LessonDate, LessonTopic, Homework, LessonTypeId, PeriodId, RoomId, SemesterId)
                OUTPUT inserted.LessonId INTO @InsertedLesson
                VALUES (@L_AssId, @CurrentDate, @LessonTopicName, @HomeworkText, @LessonTypeId, @L_Period, @RandomRoomId, @CurrentSemId);
                
                DECLARE @NewLId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM @InsertedLesson);
                
                IF @CurrentDate <= @TodaySimulated
                BEGIN
                    DECLARE st_cursor CURSOR FOR SELECT StudentId FROM [Core].[Students] WHERE ClassId = @L_ClassId;
                    OPEN st_cursor;
                    DECLARE @StId UNIQUEIDENTIFIER;
                    FETCH NEXT FROM st_cursor INTO @StId;

                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        DECLARE @GradeToInsert NVARCHAR(3) = NULL;
                        DECLARE @Rand INT = ABS(CHECKSUM(NEWId())) % 100;

                        DECLARE @AttStatus NVARCHAR(20) = NULL;
                        
                        DECLARE @SickEnd DATE = NULL;
                        SELECT TOP 1 @SickEnd = SickUntil FROM @ActiveSickness WHERE StudentId = @StId AND SickUntil >= @CurrentDate ORDER BY SickUntil DESC;
                        
                        IF @AdminUserId IS NULL SELECT TOP 1 @AdminUserId = UserId FROM [Identity].[Users] WHERE Login = 'admin';
                        IF @SickEnd IS NOT NULL AND @SickEnd >= @CurrentDate
                        BEGIN
                            SET @AttStatus = N'хв'; 
                        END
                        ELSE
                        BEGIN
                            IF ABS(CHECKSUM(NEWId())) % 1000 < 3
                            BEGIN
                                INSERT INTO @ActiveSickness (StudentId, SickUntil) VALUES (@StId, DATEADD(DAY, 5, @CurrentDate));
                                SET @AttStatus = N'хв';
                            END
                            ELSE IF @Rand > 95
                            BEGIN
                                SET @AttStatus = N'Н';
                            END
                        END

IF @AttStatus IS NOT NULL
                        BEGIN
                            INSERT INTO [Operations].[Attendances] (LessonId, StudentId, Status)
                            VALUES (@NewLId, @StId, @AttStatus);
                        END
                        ELSE
                        BEGIN
                            IF @LessonTypeId = @LT_Assessment
                            BEGIN
                                SET @GradeToInsert = CAST((6 + ABS(CHECKSUM(NEWId())) % 7) AS NVARCHAR);
                            END
                            
                            IF @GradeToInsert IS NOT NULL
                            BEGIN
                                INSERT INTO [Operations].[Grades] (LessonId, StudentId, GradeValue, Comment, CreatedByUserId, UpdatedByUserId, GradeTypeId)
                                VALUES (@NewLId, @StId, @GradeToInsert, NULL, @AdminUserId, @AdminUserId, @GradeType);
                            END
                        END
                        
                        FETCH NEXT FROM st_cursor INTO @StId;
                    END
                    CLOSE st_cursor; DEALLOCATE st_cursor;
                END
                FETCH NEXT FROM lessons_cursor_L INTO @L_AssId, @L_SubjName, @L_SubjId, @L_ClassId, @L_Period;
            END
            CLOSE lessons_cursor_L; DEALLOCATE lessons_cursor_L;
        END
    END 
    SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
END

COMMIT TRAN;

DROP TABLE #Topics;
GO


PRINT N'=== 10. Налаштування безпеки (PoP: Least Privilege) ===';
IF DATABASE_PRINCIPAL_ID('ApiApplicationRole') IS NOT NULL
    DROP ROLE [ApiApplicationRole];
GO
CREATE ROLE [ApiApplicationRole];
GO

IF SUSER_ID('User_Identity') IS NOT NULL
    DROP LOGIN [User_Identity];
GO
CREATE LOGIN [User_Identity] WITH PASSWORD = 'YourStrongPassword!', CHECK_POLICY = OFF;
GO

IF USER_ID('User_Identity') IS NOT NULL
    DROP USER [User_Identity];
GO
CREATE USER [User_Identity] FOR LOGIN [User_Identity];
GO

ALTER ROLE [ApiApplicationRole] ADD MEMBER [User_Identity];
GO


GRANT SELECT, INSERT ON [Infrastructure].[AuditLogs] TO [ApiApplicationRole];

GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Core] TO [ApiApplicationRole];
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Identity] TO [ApiApplicationRole];
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Operations] TO [ApiApplicationRole];
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Communications] TO [ApiApplicationRole];

GRANT SELECT, INSERT ON [Infrastructure].[AuditLogs] TO [ApiApplicationRole];
GRANT SELECT, INSERT, UPDATE, DELETE ON [Infrastructure].[OutboxMessages] TO [ApiApplicationRole];
GRANT SELECT, UPDATE ON [Infrastructure].[SystemSettings] TO [ApiApplicationRole];

GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Reference] TO [ApiApplicationRole];

DENY ALTER ANY SCHEMA TO [ApiApplicationRole];

GO

PRINT N'=== ГОТОВО! ===';

DECLARE @TestStudentId UNIQUEIDENTIFIER;
SELECT @TestStudentId = UserId FROM [Identity].[Users] WHERE Login = 'student';
UPDATE TOP (1) [Core].[Students] SET UserId = @TestStudentId;

DECLARE @ParentUserId UNIQUEIDENTIFIER = NEWId();
DECLARE @ParentRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM [Identity].[Roles] WHERE RoleName = N'Parent');

INSERT INTO [Identity].[Users] (UserId, Login, Email, PasswordHash, RoleId) 
VALUES (@ParentUserId, 'parent', 'parent@school.ua', 'BgpoOZJKmgihrXwGiKCovg==:gdHBs4uwY7c+6QNIuc/yfiICHPcJ7frcwO4zRDoBnXk=', @ParentRoleId);

UPDATE [Core].[Parents] SET UserId = @ParentUserId WHERE Phone LIKE '%50%1000000%';

DECLARE @FinalAdminId UNIQUEIDENTIFIER = (SELECT TOP 1 UserId FROM [Identity].[Users] WHERE Login = 'admin');
UPDATE [Communications].[Announcements] SET AuthorId = @FinalAdminId;
UPDATE [Infrastructure].[OutboxMessages] SET CreatedByUserId = @FinalAdminId;
UPDATE [Infrastructure].[SystemSettings] SET UpdatedByUserId = @FinalAdminId;

ALTER TABLE [Communications].[Announcements] ADD CONSTRAINT FK_Announcements_Users FOREIGN KEY (AuthorId) REFERENCES [Identity].[Users](UserId);
ALTER TABLE [Infrastructure].[OutboxMessages] ADD CONSTRAINT FK_OutboxMessages_Users FOREIGN KEY (CreatedByUserId) REFERENCES [Identity].[Users](UserId);
ALTER TABLE [Infrastructure].[SystemSettings] ADD CONSTRAINT FK_SystemSettings_Users FOREIGN KEY (UpdatedByUserId) REFERENCES [Identity].[Users](UserId);


PRINT N'=== 11. Стиснення всіх індексів та таблиць ==='

DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql += 'ALTER TABLE [' + s.name + '].[' + t.name + '] REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = PAGE); ' +
               'ALTER INDEX ALL ON [' + s.name + '].[' + t.name + '] REBUILD WITH (DATA_COMPRESSION = PAGE); '
FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id;
EXEC sys.sp_executesql @sql;
GO

PRINT N'=== 12. Глобальні авто-фікси (Descriptions, Soft Delete, History) ===';
DECLARE @sqlHacks NVARCHAR(MAX) = '';

SELECT @sqlHacks += 'EXEC sys.sp_addextendedproperty @name=N''MS_Description'', @value=N''Автоопис'', @level0type=N''SCHEMA'', @level0name=N''' + s.name + ''', @level1type=N''TABLE'', @level1name=N''' + t.name + '''; '
FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.is_ms_shipped = 0 AND NOT EXISTS (SELECT 1 FROM sys.extended_properties ep WHERE ep.major_id = t.object_id AND ep.minor_id = 0 AND ep.name = 'MS_Description');

SELECT @sqlHacks += 'EXEC sys.sp_addextendedproperty @name=N''MS_Description'', @value=N''Автоопис'', @level0type=N''SCHEMA'', @level0name=N''' + s.name + ''', @level1type=N''TABLE'', @level1name=N''' + t.name + ''', @level2type=N''COLUMN'', @level2name=N''' + c.name + '''; '
FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.is_ms_shipped = 0 AND NOT EXISTS (SELECT 1 FROM sys.extended_properties ep WHERE ep.major_id = t.object_id AND ep.minor_id = c.column_id AND ep.name = 'MS_Description');

SELECT @sqlHacks += 'CREATE UNIQUE NONCLUSTERED INDEX IX_SoftDel_' + t.name + ' ON [' + s.name + '].[' + t.name + '](' + c.name + ') WHERE IsDeleted = 0 WITH (DATA_COMPRESSION = PAGE); '
FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
JOIN sys.indexes i ON t.object_id = i.object_id AND i.is_primary_key = 1
JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id AND ic.key_ordinal = 1
JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE EXISTS (SELECT 1 FROM sys.columns del WHERE del.object_id = t.object_id AND del.name = 'IsDeleted')
  AND t.name NOT LIKE '%History%'
  AND NOT EXISTS (
      SELECT 1 FROM sys.indexes existing 
      WHERE existing.object_id = t.object_id AND existing.is_unique = 1 AND existing.has_filter = 1
  );

EXEC sys.sp_executesql @sqlHacks;
GO

DECLARE @historyFix NVARCHAR(MAX) = N'
IF OBJECT_Id(''[Operations].[Grades_EFMigrationsHistory]'') IS NOT NULL
BEGIN
    ALTER TABLE [Operations].[Grades] SET (SYSTEM_VERSIONING = OFF);
    ALTER TABLE [Operations].[Grades_EFMigrationsHistory] ADD CONSTRAINT DF_GH_IsDel DEFAULT 0 FOR IsDeleted;
    ALTER TABLE [Operations].[Grades_EFMigrationsHistory] ADD CONSTRAINT DF_GH_CA DEFAULT GETUTCDATE() FOR CreatedAt;
    CREATE NONCLUSTERED INDEX IX_GH_IsDel ON [Operations].[Grades_EFMigrationsHistory](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
    ALTER TABLE [Operations].[Grades] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [Operations].[Grades_EFMigrationsHistory]));
END

IF OBJECT_Id(''[Core].[Students_EFMigrationsHistory]'') IS NOT NULL
BEGIN
    ALTER TABLE [Core].[Students] SET (SYSTEM_VERSIONING = OFF);
    CREATE NONCLUSTERED INDEX IX_SH_IsDel ON [Core].[Students_EFMigrationsHistory](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
    ALTER TABLE [Core].[Students] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [Core].[Students_EFMigrationsHistory]));
END

IF OBJECT_Id(''[Operations].[Schedules_EFMigrationsHistory]'') IS NOT NULL
BEGIN
    CREATE NONCLUSTERED INDEX IX_SchH_IsDel ON [Operations].[Schedules_EFMigrationsHistory](IsDeleted) WITH (DATA_COMPRESSION = PAGE);
END
';
EXEC sys.sp_executesql @historyFix;
GO

SELECT 'ACCOUNT CHECK' AS Info, Login, RoleId, UserId 
FROM [Identity].[Users] 
ORDER BY RoleId;

SET NOEXEC OFF;