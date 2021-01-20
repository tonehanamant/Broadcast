CREATE TABLE [dbo].[genres] (
    [id]                INT           IDENTITY (1, 1) NOT NULL,
    [name]              VARCHAR (127) NOT NULL,
    [created_by]        VARCHAR (63)  NOT NULL,
    [created_date]      DATETIME      NOT NULL,
    [modified_by]       VARCHAR (63)  NOT NULL,
    [modified_date]     DATETIME      NOT NULL,
    [program_source_id] INT           NOT NULL,
    CONSTRAINT [PK_genres] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_genres_program_sources] FOREIGN KEY ([program_source_id]) REFERENCES [dbo].[program_sources] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_genres_program_sources]
    ON [dbo].[genres]([program_source_id] ASC);

