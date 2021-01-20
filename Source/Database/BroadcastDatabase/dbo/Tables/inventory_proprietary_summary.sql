CREATE TABLE [dbo].[inventory_proprietary_summary] (
    [id]                                                INT             IDENTITY (1, 1) NOT NULL,
    [inventory_source_id]                               INT             NOT NULL,
    [quarter_number]                                    INT             NOT NULL,
    [quarter_year]                                      INT             NOT NULL,
    [unit]                                              INT             NOT NULL,
    [created_by]                                        VARCHAR (63)    NOT NULL,
    [created_at]                                        DATETIME        NOT NULL,
    [inventory_proprietary_daypart_program_mappings_id] INT             NOT NULL,
    [unit_cost]                                         DECIMAL (19, 4) NOT NULL,
    [is_active]                                         BIT             NOT NULL,
    CONSTRAINT [PK_inventory_proprietary_summary] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_proprietary_summary_inventory_proprietary_daypart_program_mappings] FOREIGN KEY ([inventory_proprietary_daypart_program_mappings_id]) REFERENCES [dbo].[inventory_proprietary_daypart_program_mappings] ([id]),
    CONSTRAINT [FK_inventory_proprietary_summary_inventory_sources] FOREIGN KEY ([inventory_source_id]) REFERENCES [dbo].[inventory_sources] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_proprietary_summary_inventory_sources]
    ON [dbo].[inventory_proprietary_summary]([inventory_source_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_proprietary_summary_inventory_proprietary_daypart_program_mappings]
    ON [dbo].[inventory_proprietary_summary]([inventory_proprietary_daypart_program_mappings_id] ASC);

