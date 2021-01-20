CREATE TABLE [dbo].[inventory_summary] (
    [id]                   INT      IDENTITY (1, 1) NOT NULL,
    [inventory_source_id]  INT      NOT NULL,
    [first_quarter_number] INT      NOT NULL,
    [first_quarter_year]   INT      NOT NULL,
    [last_quarter_number]  INT      NOT NULL,
    [last_quarter_year]    INT      NOT NULL,
    [last_update_date]     DATETIME NULL,
    CONSTRAINT [PK_inventory_summary] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_summary_inventory_sources] FOREIGN KEY ([inventory_source_id]) REFERENCES [dbo].[inventory_sources] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_summary_inventory_sources]
    ON [dbo].[inventory_summary]([inventory_source_id] ASC);

