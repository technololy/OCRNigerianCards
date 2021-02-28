IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AppruvResponses] (
    [Id] int NOT NULL IDENTITY,
    [BVN] nvarchar(11) NULL,
    [Email] nvarchar(max) NULL,
    [StatusOfRequest] nvarchar(max) NULL,
    [DateInserted] datetime NULL DEFAULT ((getdate())),
    [JsonResponse] nvarchar(max) NULL,
    CONSTRAINT [PK_AppruvResponses] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [FacialValidations] (
    [Id] int NOT NULL IDENTITY,
    [BVN] nvarchar(11) NULL,
    [Email] nvarchar(max) NULL,
    [Accessories] nvarchar(max) NULL,
    [FacialHair] nvarchar(max) NULL,
    [Hair] nvarchar(max) NULL,
    [Emotion] nvarchar(max) NULL,
    [Smile] nvarchar(max) NULL,
    [Age] nvarchar(max) NULL,
    [HeadPose] nvarchar(max) NULL,
    [Gender] nvarchar(max) NULL,
    [Occlusion] nvarchar(max) NULL,
    [DateInserted] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK_FacialValidations] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [OCRResponses] (
    [Id] int NOT NULL IDENTITY,
    [BVN] nvarchar(11) NULL,
    [JsonResponse] nvarchar(max) NULL,
    [DateInserted] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK_OCRResponses] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ScannedIDCardDetail] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(max) NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [MiddleName] nvarchar(max) NULL,
    [IDType] nvarchar(max) NULL,
    [IDNumber] nvarchar(max) NULL,
    [IssueDate] nvarchar(max) NULL,
    [DateOfBirth] nvarchar(max) NULL,
    [ExpiryDate] nvarchar(max) NULL,
    [FormerIDNumber] nvarchar(max) NULL,
    [IDClass] nvarchar(max) NULL,
    [BVN] nvarchar(11) NULL,
    [Email] nvarchar(max) NULL,
    [BloodGroup] nvarchar(max) NULL,
    [Height] nvarchar(max) NULL,
    [IssuingAuthority] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [Occupation] nvarchar(max) NULL,
    [NextOfKin] nvarchar(max) NULL,
    [FirstIssueState] nvarchar(max) NULL,
    [Delim] nvarchar(max) NULL,
    [Gender] nvarchar(max) NULL,
    [DateInserted] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK_ScannedIDCardDetail] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210213223829_InitialCreate', N'5.0.3');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [OCRUsages] (
    [Id] int NOT NULL IDENTITY,
    [Count] int NOT NULL,
    [EmailAddress] nvarchar(max) NULL,
    [DateCreated] datetime2 NOT NULL,
    [DateModified] datetime2 NOT NULL,
    [IsEnabled] bit NOT NULL,
    CONSTRAINT [PK_OCRUsages] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210228195258_OCRUsage', N'5.0.3');
GO

COMMIT;
GO

