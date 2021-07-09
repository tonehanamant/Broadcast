CREATE TABLE [dbo].[export_unmapped_program_names_jobs]
(
	[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
	[status] INT NOT NULL,
	[queued_at] DATETIME2 NOT NULL,
	[queued_by] VARCHAR(100) NOT NULL,
	[completed_at] DATETIME2 NULL,
	[error_message] NVARCHAR(MAX) NULL
);
