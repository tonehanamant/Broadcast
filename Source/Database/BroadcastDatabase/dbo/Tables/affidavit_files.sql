CREATE TABLE [dbo].[affidavit_files] (
    [id]           INT           IDENTITY (1, 1) NOT NULL,
    [file_name]    VARCHAR (255) NOT NULL,
    [file_hash]    VARCHAR (255) NOT NULL,
    [source_id]    INT           NOT NULL,
    [created_date] DATETIME      NOT NULL,
    [status]       INT           NOT NULL,
    CONSTRAINT [PK_affidavit_files] PRIMARY KEY CLUSTERED ([id] ASC)
);

