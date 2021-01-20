CREATE TABLE [dbo].[inventory_proprietary_summary_station_audiences] (
    [id]                               INT             IDENTITY (1, 1) NOT NULL,
    [inventory_proprietary_summary_id] INT             NOT NULL,
    [audience_id]                      INT             NOT NULL,
    [market_code]                      SMALLINT        NOT NULL,
    [station_id]                       INT             NOT NULL,
    [impressions]                      FLOAT (53)      NOT NULL,
    [spots_per_week]                   INT             NOT NULL,
    [cost_per_week]                    DECIMAL (19, 4) NOT NULL,
    CONSTRAINT [PK_inventory_proprietary_summary_station_audiences] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_proprietary_summary_station_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_inventory_proprietary_summary_station_audiences_inventory_proprietary_summary] FOREIGN KEY ([inventory_proprietary_summary_id]) REFERENCES [dbo].[inventory_proprietary_summary] ([id]),
    CONSTRAINT [FK_inventory_proprietary_summary_station_audiences_markets] FOREIGN KEY ([market_code]) REFERENCES [dbo].[markets] ([market_code]),
    CONSTRAINT [FK_inventory_proprietary_summary_station_audiences_stations] FOREIGN KEY ([station_id]) REFERENCES [dbo].[stations] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_proprietary_summary_station_audiences_inventory_proprietary_summary]
    ON [dbo].[inventory_proprietary_summary_station_audiences]([inventory_proprietary_summary_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_proprietary_summary_station_audiences_stations]
    ON [dbo].[inventory_proprietary_summary_station_audiences]([station_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_proprietary_summary_station_audiences_markets]
    ON [dbo].[inventory_proprietary_summary_station_audiences]([market_code] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_proprietary_summary_station_audiences_audiences]
    ON [dbo].[inventory_proprietary_summary_station_audiences]([audience_id] ASC);

