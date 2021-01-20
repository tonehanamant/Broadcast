CREATE TABLE [dbo].[inventory_proprietary_daypart_programs] (
    [id]           INT           IDENTITY (1, 1) NOT NULL,
    [unit_type]    VARCHAR (50)  NOT NULL,
    [program_name] VARCHAR (150) NOT NULL,
    [created_by]   VARCHAR (63)  NOT NULL,
    [created_at]   DATETIME      NOT NULL,
    [modified_by]  VARCHAR (63)  NULL,
    [modified_at]  DATETIME      NULL,
    [genre_id]     INT           NOT NULL,
    [show_type_id] INT           NOT NULL,
    CONSTRAINT [PK_inventory_proprietary_daypart_programs] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_proprietary_daypart_programs_genres] FOREIGN KEY ([genre_id]) REFERENCES [dbo].[genres] ([id]),
    CONSTRAINT [FK_inventory_proprietary_daypart_programs_show_types] FOREIGN KEY ([show_type_id]) REFERENCES [dbo].[show_types] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_proprietary_daypart_programs_show_types]
    ON [dbo].[inventory_proprietary_daypart_programs]([show_type_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_proprietary_daypart_programs_genres]
    ON [dbo].[inventory_proprietary_daypart_programs]([genre_id] ASC);

