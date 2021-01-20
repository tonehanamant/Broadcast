CREATE TABLE [dbo].[show_types] (
    [id]                INT           IDENTITY (1, 1) NOT NULL,
    [name]              VARCHAR (127) NOT NULL,
    [created_by]        VARCHAR (63)  NOT NULL,
    [created_date]      DATETIME      NOT NULL,
    [modified_by]       VARCHAR (63)  NOT NULL,
    [modified_date]     DATETIME      NOT NULL,
    [program_source_id] INT           NOT NULL,
    CONSTRAINT [PK_show_types] PRIMARY KEY CLUSTERED ([id] ASC)
);

