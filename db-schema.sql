-- CommentsApp database schema (SQL Server)
-- This file reflects the current EF Core migrations in the repository.

CREATE TABLE [dbo].[Comments] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserName] NVARCHAR(MAX) NOT NULL,
    [Email] NVARCHAR(MAX) NOT NULL,
    [HomePage] NVARCHAR(MAX) NULL,
    [Text] NVARCHAR(MAX) NOT NULL,
    [ParentId] INT NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    CONSTRAINT [FK_Comments_Comments_ParentId]
        FOREIGN KEY ([ParentId]) REFERENCES [dbo].[Comments]([Id])
        ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_Comments_ParentId] ON [dbo].[Comments]([ParentId]);
GO

CREATE TABLE [dbo].[CommentAttachment] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CommentId] INT NOT NULL,
    [FileName] NVARCHAR(255) NOT NULL,
    [Path] NVARCHAR(500) NOT NULL,
    [Size] BIGINT NOT NULL,
    [ContentType] NVARCHAR(100) NOT NULL,
    [IsImage] BIT NOT NULL,
    CONSTRAINT [FK_CommentAttachment_Comments_CommentId]
        FOREIGN KEY ([CommentId]) REFERENCES [dbo].[Comments]([Id])
        ON DELETE CASCADE
);
GO

CREATE INDEX [IX_CommentAttachment_CommentId] ON [dbo].[CommentAttachment]([CommentId]);
GO
