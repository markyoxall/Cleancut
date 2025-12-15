-- SQL script to create IdempotencyRecords table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IdempotencyRecords]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[IdempotencyRecords]
    (
        [Key] NVARCHAR(200) NOT NULL PRIMARY KEY,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [UserId] NVARCHAR(MAX) NULL,
        [RequestHash] NVARCHAR(1000) NULL,
        [ResponsePayload] NVARCHAR(MAX) NULL,
        [ResponseStatus] INT NULL,
        [ResponseHeaders] NVARCHAR(MAX) NULL,
        [RowVersion] ROWVERSION NOT NULL
    );

    CREATE INDEX IX_IdempotencyRecords_CreatedAt ON [dbo].[IdempotencyRecords]([CreatedAt]);
END
GO
