
-- SQL Server schema generation script
-- Generated from .NET project structure

-- ActivityLogs Table
CREATE TABLE [ActivityLogs] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Timestamp] DATETIME2 NOT NULL,
    [Level] NVARCHAR(MAX) NULL,
    [MessageTemplate] NVARCHAR(MAX) NULL,
    [Message] NVARCHAR(MAX) NULL,
    [Exception] NVARCHAR(MAX) NULL,
    [Properties] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_ActivityLogs] PRIMARY KEY ([Id])
);

-- ApprovalHistories Table
CREATE TABLE [ApprovalHistories] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [SupportRequestId] INT NOT NULL,
    [ApproverId] INT NOT NULL,
    [Status] NVARCHAR(MAX) NOT NULL,
    [Comments] NVARCHAR(MAX) NULL,
    [ApprovalDate] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedBy] NVARCHAR(MAX) NULL,
    [UpdatedAt] DATETIME2 NOT NULL,
    [UpdatedBy] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_ApprovalHistories] PRIMARY KEY ([Id])
);

-- ApprovalSequences Table
CREATE TABLE [ApprovalSequences] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [DepartmentId] INT NOT NULL,
    [SectionId] INT NOT NULL,
    [Sequence] INT NOT NULL,
    [ApproverId] INT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedBy] NVARCHAR(MAX) NULL,
    [UpdatedAt] DATETIME2 NOT NULL,
    [UpdatedBy] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_ApprovalSequences] PRIMARY KEY ([Id])
);

-- Departments Table
CREATE TABLE [Departments] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(MAX) NOT NULL,
    [Status] NVARCHAR(MAX) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedBy] NVARCHAR(MAX) NULL,
    [UpdatedAt] DATETIME2 NOT NULL,
    [UpdatedBy] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
);

-- Menus Table
CREATE TABLE [Menus] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(MAX) NOT NULL,
    [Url] NVARCHAR(MAX) NOT NULL,
    [ParentId] INT NULL,
    [Icon] NVARCHAR(MAX) NULL,
    [Order] INT NOT NULL,
    CONSTRAINT [PK_Menus] PRIMARY KEY ([Id])
);

-- News Table
CREATE TABLE [News] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Title] NVARCHAR(MAX) NOT NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [ImagePath] NVARCHAR(MAX) NULL,
    [IsPublished] BIT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedBy] NVARCHAR(MAX) NULL,
    [UpdatedAt] DATETIME2 NOT NULL,
    [UpdatedBy] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_News] PRIMARY KEY ([Id])
);

-- RoleMenus Table
CREATE TABLE [RoleMenus] (
    [RoleId] INT NOT NULL,
    [MenuId] INT NOT NULL,
    CONSTRAINT [PK_RoleMenus] PRIMARY KEY ([RoleId], [MenuId])
);

-- Sections Table
CREATE TABLE [Sections] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(MAX) NOT NULL,
    [DepartmentId] INT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedBy] NVARCHAR(MAX) NULL,
    [UpdatedAt] DATETIME2 NOT NULL,
    [UpdatedBy] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_Sections] PRIMARY KEY ([Id])
);

-- SupportRequests Table
CREATE TABLE [SupportRequests] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [DocumentNo] NVARCHAR(MAX) NULL,
    [RequestDate] DATETIME2 NOT NULL,
    [RequesterId] INT NOT NULL,
    [Department] NVARCHAR(MAX) NULL,
    [Section] NVARCHAR(MAX) NULL,
    [Tel] NVARCHAR(MAX) NULL,
    [Subject] NVARCHAR(MAX) NOT NULL,
    [Details] NVARCHAR(MAX) NOT NULL,
    [Status] NVARCHAR(MAX) NOT NULL,
    [CurrentApproverId] INT NULL,
    [ClosedDate] DATETIME2 NULL,
    [WorkStartDate] DATETIME2 NULL,
    [WorkEndDate] DATETIME2 NULL,
    [WorkDetails] NVARCHAR(MAX) NULL,
    [AssignedToId] INT NULL,
    [SLA] INT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedBy] NVARCHAR(MAX) NULL,
    [UpdatedAt] DATETIME2 NOT NULL,
    [UpdatedBy] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_SupportRequests] PRIMARY KEY ([Id])
);

-- Users Table
CREATE TABLE [Users] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Username] NVARCHAR(MAX) NOT NULL,
    [Password] NVARCHAR(MAX) NOT NULL,
    [FirstName] NVARCHAR(MAX) NOT NULL,
    [LastName] NVARCHAR(MAX) NOT NULL,
    [EmployeeId] NVARCHAR(MAX) NOT NULL,
    [Email] NVARCHAR(MAX) NOT NULL,
    [Department] NVARCHAR(MAX) NULL,
    [Section] NVARCHAR(MAX) NULL,
    [Role] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

-- WorkItems Table
CREATE TABLE [WorkItems] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Title] NVARCHAR(MAX) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [IsUrgent] BIT NOT NULL,
    [RequestedBy] NVARCHAR(MAX) NOT NULL,
    [Status] INT NOT NULL,
    [SapNo] NVARCHAR(MAX) NULL,
    [PartNumber] NVARCHAR(MAX) NULL,
    [PartName] NVARCHAR(MAX) NULL,
    [Quantity] INT NULL,
    [CostCenter] NVARCHAR(MAX) NULL,
    [Approver] NVARCHAR(MAX) NULL,
    [ApprovalDate] DATETIME2 NULL,
    [EnglishMatDescription] NVARCHAR(MAX) NULL,
    [ThaiMatDescription] NVARCHAR(MAX) NULL,
    [ICSCode] NVARCHAR(MAX) NULL,
    [MakerMfrPartNumber] NVARCHAR(MAX) NULL,
    [SpeacialFeature] NVARCHAR(MAX) NULL,
    [EmployeeId] NVARCHAR(MAX) NOT NULL,
    [MRPController] NVARCHAR(MAX) NULL,
    [IsFG] BIT NOT NULL,
    [IsSM] BIT NOT NULL,
    [IsRM] BIT NOT NULL,
    [IsTooling] BIT NOT NULL,
    [AlternativeA] NVARCHAR(MAX) NULL,
    [AlternativeB] NVARCHAR(MAX) NULL,
    [AssignedTo] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [CreatedBy] NVARCHAR(MAX) NULL,
    [UpdatedAt] DATETIME2 NOT NULL,
    [UpdatedBy] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_WorkItems] PRIMARY KEY ([Id])
);
