CREATE TABLE [dbo].[inventory_summary_quarter_details] (
    [id]                           INT             IDENTITY (1, 1) NOT NULL,
    [inventory_summary_quarter_id] INT             NOT NULL,
    [total_markets]                INT             NOT NULL,
    [total_coverage]               FLOAT (53)      NOT NULL,
    [total_units]                  INT             NULL,
    [total_programs]               INT             NULL,
    [total_projected_impressions]  FLOAT (53)      NULL,
    [cpm]                          DECIMAL (19, 4) NULL,
    [min_spots_per_week]           INT             NULL,
    [max_spots_per_week]           INT             NULL,
    [standard_daypart_id]          INT             NOT NULL,
    CONSTRAINT [PK_inventory_summary_quarter_details] PRIMARY KEY CLUSTERED ([id] ASC, [inventory_summary_quarter_id] ASC),
    CONSTRAINT [FK_inventory_summary_quarter_details_inventory_summary_quarters] FOREIGN KEY ([inventory_summary_quarter_id]) REFERENCES [dbo].[inventory_summary_quarters] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_inventory_summary_quarter_details_standard_dayparts] FOREIGN KEY ([standard_daypart_id]) REFERENCES [dbo].[standard_dayparts] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_summary_quarter_details_standard_dayparts]
    ON [dbo].[inventory_summary_quarter_details]([standard_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_summary_quarter_details_inventory_summary_quarters]
    ON [dbo].[inventory_summary_quarter_details]([inventory_summary_quarter_id] ASC);

