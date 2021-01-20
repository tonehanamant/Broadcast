CREATE TABLE [dbo].[inventory_summary_gap_ranges] (
    [id]                        INT      IDENTITY (1, 1) NOT NULL,
    [inventory_summary_gaps_id] INT      NOT NULL,
    [start_date]                DATETIME NOT NULL,
    [end_date]                  DATETIME NOT NULL,
    CONSTRAINT [PK_inventory_summary_gap_ranges] PRIMARY KEY CLUSTERED ([id] ASC, [inventory_summary_gaps_id] ASC),
    CONSTRAINT [FK_inventory_summary_gap_ranges_inventory_summary_gaps] FOREIGN KEY ([inventory_summary_gaps_id]) REFERENCES [dbo].[inventory_summary_gaps] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_summary_gap_ranges_inventory_summary_gaps]
    ON [dbo].[inventory_summary_gap_ranges]([inventory_summary_gaps_id] ASC);

