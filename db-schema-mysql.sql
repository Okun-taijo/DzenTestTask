-- CommentsApp database schema (MySQL 8.x / MySQL Workbench)
-- Logical equivalent of the SQL Server schema used in the project.

CREATE DATABASE IF NOT EXISTS comments_app
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE comments_app;

CREATE TABLE IF NOT EXISTS Comments (
    Id INT NOT NULL AUTO_INCREMENT,
    UserName LONGTEXT NOT NULL,
    Email LONGTEXT NOT NULL,
    HomePage LONGTEXT NULL,
    Text LONGTEXT NOT NULL,
    ParentId INT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    PRIMARY KEY (Id),
    INDEX IX_Comments_ParentId (ParentId),
    CONSTRAINT FK_Comments_Comments_ParentId
        FOREIGN KEY (ParentId) REFERENCES Comments(Id)
        ON DELETE RESTRICT
        ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS CommentAttachment (
    Id INT NOT NULL AUTO_INCREMENT,
    CommentId INT NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    Path VARCHAR(500) NOT NULL,
    Size BIGINT NOT NULL,
    ContentType VARCHAR(100) NOT NULL,
    IsImage BOOLEAN NOT NULL,
    PRIMARY KEY (Id),
    INDEX IX_CommentAttachment_CommentId (CommentId),
    CONSTRAINT FK_CommentAttachment_Comments_CommentId
        FOREIGN KEY (CommentId) REFERENCES Comments(Id)
        ON DELETE CASCADE
        ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
