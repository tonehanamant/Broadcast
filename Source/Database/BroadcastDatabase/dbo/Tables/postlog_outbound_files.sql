CREATE TABLE [dbo].[postlog_outbound_files] (
    [id]           INT           IDENTITY (1, 1) NOT NULL,
    [file_name]    VARCHAR (255) NOT NULL,
    [file_hash]    VARCHAR (63)  NOT NULL,
    [source_id]    INT           NOT NULL,
    [status]       INT           NOT NULL,
    [created_date] DATETIME      NOT NULL,
    [created_by]   VARCHAR (63)  NOT NULL,
    CONSTRAINT [PK_postlog_outbound_files] PRIMARY KEY CLUSTERED ([id] ASC)
);

