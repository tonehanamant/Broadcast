CREATE TABLE [dbo].[real_isci_ingest_jobs]
(
	[id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
	[status] INT NOT NULL,
	[queued_at] DATETIME2 NOT NULL,
	[completed_at] DATETIME2 NULL,
	[error_message] NVARCHAR(MAX) NULL
);
