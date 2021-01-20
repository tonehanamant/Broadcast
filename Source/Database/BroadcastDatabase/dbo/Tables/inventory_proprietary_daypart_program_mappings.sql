CREATE TABLE [dbo].[inventory_proprietary_daypart_program_mappings] (
    [id]                                        INT          IDENTITY (1, 1) NOT NULL,
    [inventory_source_id]                       INT          NOT NULL,
    [inventory_proprietary_daypart_programs_id] INT          NOT NULL,
    [created_by]                                VARCHAR (63) NOT NULL,
    [created_at]                                DATETIME     NOT NULL,
    [modified_by]                               VARCHAR (63) NULL,
    [modified_at]                               DATETIME     NULL,
    [standard_daypart_id]                       INT          NOT NULL,
    CONSTRAINT [PK_inventory_proprietary_daypart_program_mappings] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_proprietary_daypart_program_mappings_inventory_proprietary_daypart_programs] FOREIGN KEY ([inventory_proprietary_daypart_programs_id]) REFERENCES [dbo].[inventory_proprietary_daypart_programs] ([id]),
    CONSTRAINT [FK_inventory_proprietary_daypart_program_mappings_inventory_sources] FOREIGN KEY ([inventory_source_id]) REFERENCES [dbo].[inventory_sources] ([id]),
    CONSTRAINT [FK_inventory_proprietary_daypart_program_mappings_standard_dayparts] FOREIGN KEY ([standard_daypart_id]) REFERENCES [dbo].[standard_dayparts] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_proprietary_daypart_program_mappings_standard_dayparts]
    ON [dbo].[inventory_proprietary_daypart_program_mappings]([standard_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_proprietary_daypart_program_mappings_inventory_proprietary_daypart_programs]
    ON [dbo].[inventory_proprietary_daypart_program_mappings]([inventory_proprietary_daypart_programs_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_proprietary_daypart_program_mappings_inventory_sources]
    ON [dbo].[inventory_proprietary_daypart_program_mappings]([inventory_source_id] ASC);

