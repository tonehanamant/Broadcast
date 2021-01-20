CREATE TABLE [dbo].[pricing_guide_distribution_open_market_inventory] (
    [id]                                     INT             IDENTITY (1, 1) NOT NULL,
    [pricing_guide_distribution_id]          INT             NOT NULL,
    [market_code]                            SMALLINT        NOT NULL,
    [station_inventory_manifest_dayparts_id] INT             NOT NULL,
    [daypart_id]                             INT             NOT NULL,
    [program_name]                           VARCHAR (MAX)   NULL,
    [blended_cpm]                            DECIMAL (19, 4) NOT NULL,
    [spots]                                  INT             NOT NULL,
    [forecasted_impressions_per_spot]        FLOAT (53)      NOT NULL,
    [station_impressions_per_spot]           FLOAT (53)      NOT NULL,
    [cost_per_spot]                          DECIMAL (19, 4) NOT NULL,
    [manifest_id]                            INT             NOT NULL,
    [station_id]                             INT             NOT NULL,
    CONSTRAINT [PK_pricing_guide_distribution_open_market_inventory] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_pricing_guide_distribution_open_market_inventory_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_pricing_guide_distribution_open_market_inventory_markets] FOREIGN KEY ([market_code]) REFERENCES [dbo].[markets] ([market_code]) ON DELETE CASCADE,
    CONSTRAINT [FK_pricing_guide_distribution_open_market_inventory_pricing_guide_distribution] FOREIGN KEY ([pricing_guide_distribution_id]) REFERENCES [dbo].[pricing_guide_distributions] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_pricing_guide_distribution_open_market_inventory_stations] FOREIGN KEY ([station_id]) REFERENCES [dbo].[stations] ([id]),
    CONSTRAINT [FK_pricing_guide_distribution_open_market_station_inventory_manifest] FOREIGN KEY ([manifest_id]) REFERENCES [dbo].[station_inventory_manifest] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_pricing_guide_distribution_open_market_inventory_stations]
    ON [dbo].[pricing_guide_distribution_open_market_inventory]([station_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_pricing_guide_distribution_open_market_inventory_markets]
    ON [dbo].[pricing_guide_distribution_open_market_inventory]([market_code] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_pricing_guide_distribution_open_market_inventory_pricing_guide_distribution]
    ON [dbo].[pricing_guide_distribution_open_market_inventory]([pricing_guide_distribution_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_pricing_guide_distribution_open_market_station_inventory_manifest]
    ON [dbo].[pricing_guide_distribution_open_market_inventory]([manifest_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_pricing_guide_distribution_open_market_inventory_dayparts]
    ON [dbo].[pricing_guide_distribution_open_market_inventory]([daypart_id] ASC);

