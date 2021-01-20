CREATE TABLE [dbo].[program_name_mappings] (
    [id]                     INT            IDENTITY (1, 1) NOT NULL,
    [inventory_program_name] NVARCHAR (500) NULL,
    [official_program_name]  NVARCHAR (500) NULL,
    [genre_id]               INT            NOT NULL,
    [show_type_id]           INT            NOT NULL,
    [created_by]             VARCHAR (63)   NOT NULL,
    [created_at]             DATETIME       NOT NULL,
    [modified_by]            VARCHAR (63)   NULL,
    [modified_at]            DATETIME       NULL,
    CONSTRAINT [PK_program_name_mappings] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_program_name_mappings_genres] FOREIGN KEY ([genre_id]) REFERENCES [dbo].[genres] ([id]),
    CONSTRAINT [FK_program_name_mappings_show_types] FOREIGN KEY ([show_type_id]) REFERENCES [dbo].[show_types] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_program_name_mappings_genres]
    ON [dbo].[program_name_mappings]([genre_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_program_name_mappings_show_types]
    ON [dbo].[program_name_mappings]([show_type_id] ASC);

