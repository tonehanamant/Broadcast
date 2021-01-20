CREATE TABLE [dbo].[inventory_summary_quarters] (
    [id]                          INT             IDENTITY (1, 1) NOT NULL,
    [inventory_source_id]         INT             NOT NULL,
    [quarter_number]              INT             NOT NULL,
    [quarter_year]                INT             NOT NULL,
    [share_book_id]               INT             NULL,
    [hut_book_id]                 INT             NULL,
    [total_markets]               INT             NOT NULL,
    [total_stations]              INT             NOT NULL,
    [total_programs]              INT             NULL,
    [total_daypart_codes]         INT             NULL,
    [total_units]                 INT             NULL,
    [total_projected_impressions] FLOAT (53)      NULL,
    [cpm]                         DECIMAL (19, 4) NULL,
    CONSTRAINT [PK_inventory_summary_quarters] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_summary_quarters_inventory_sources] FOREIGN KEY ([inventory_source_id]) REFERENCES [dbo].[inventory_sources] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_summary_quarters_inventory_sources]
    ON [dbo].[inventory_summary_quarters]([inventory_source_id] ASC);

