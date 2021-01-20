CREATE TABLE [dbo].[program_name_mapping_keywords] (
    [id]           INT           IDENTITY (1, 1) NOT NULL,
    [keyword]      VARCHAR (100) NOT NULL,
    [program_name] VARCHAR (100) NOT NULL,
    [genre_id]     INT           NOT NULL,
    [show_type_id] INT           NOT NULL,
    CONSTRAINT [PK_program_name_mapping_keywords] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_program_name_mapping_keywords_genres] FOREIGN KEY ([genre_id]) REFERENCES [dbo].[genres] ([id]),
    CONSTRAINT [FK_program_name_mapping_keywords_show_types] FOREIGN KEY ([show_type_id]) REFERENCES [dbo].[show_types] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_program_name_mapping_keywords_show_types]
    ON [dbo].[program_name_mapping_keywords]([show_type_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_program_name_mapping_keywords_genres]
    ON [dbo].[program_name_mapping_keywords]([genre_id] ASC);

