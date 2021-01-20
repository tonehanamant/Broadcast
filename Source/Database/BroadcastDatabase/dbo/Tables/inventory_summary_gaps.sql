CREATE TABLE [dbo].[inventory_summary_gaps] (
    [id]                   INT IDENTITY (1, 1) NOT NULL,
    [quarter_number]       INT NOT NULL,
    [quarter_year]         INT NOT NULL,
    [all_quarter_missing]  BIT NOT NULL,
    [inventory_summary_id] INT NOT NULL,
    CONSTRAINT [PK_inventory_summary_gaps] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_summary_gaps_inventory_summary1] FOREIGN KEY ([inventory_summary_id]) REFERENCES [dbo].[inventory_summary] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_summary_gaps_inventory_summary1]
    ON [dbo].[inventory_summary_gaps]([inventory_summary_id] ASC);

