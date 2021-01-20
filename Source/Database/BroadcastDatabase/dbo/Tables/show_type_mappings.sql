CREATE TABLE [dbo].[show_type_mappings] (
    [maestro_show_type_id] INT          NOT NULL,
    [mapped_show_type_id]  INT          NOT NULL,
    [created_by]           VARCHAR (63) NOT NULL,
    [created_date]         DATETIME     NOT NULL,
    [modified_by]          VARCHAR (63) NOT NULL,
    [modified_date]        DATETIME     NOT NULL,
    CONSTRAINT [PK_show_type_mappings] PRIMARY KEY CLUSTERED ([maestro_show_type_id] ASC, [mapped_show_type_id] ASC),
    CONSTRAINT [FK_show_type_mappings_show_types_maestro] FOREIGN KEY ([maestro_show_type_id]) REFERENCES [dbo].[show_types] ([id]),
    CONSTRAINT [FK_show_type_mappings_show_types_mapped] FOREIGN KEY ([mapped_show_type_id]) REFERENCES [dbo].[show_types] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_show_type_mappings_show_types_mapped]
    ON [dbo].[show_type_mappings]([mapped_show_type_id] ASC);

