CREATE TABLE [dbo].[station_inventory_spot_snapshots] (
    [id]                                      INT             IDENTITY (1, 1) NOT NULL,
    [proposal_version_detail_quarter_week_id] INT             NULL,
    [media_week_id]                           INT             NOT NULL,
    [spot_length_id]                          INT             NOT NULL,
    [program_name]                            VARCHAR (255)   NULL,
    [daypart_id]                              INT             NOT NULL,
    [station_call_letters]                    VARCHAR (15)    NOT NULL,
    [station_market_code]                     SMALLINT        NOT NULL,
    [station_market_rank]                     INT             NOT NULL,
    [spot_impressions]                        FLOAT (53)      NULL,
    [spot_cost]                               DECIMAL (19, 4) NULL,
    [audience_id]                             INT             NOT NULL,
    [station_id]                              INT             NOT NULL,
    CONSTRAINT [PK_station_inventory_spot_snapshots] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_inventory_spot_snapshots_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_station_inventory_spot_snapshots_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_station_inventory_spot_snapshots_markets] FOREIGN KEY ([station_market_code]) REFERENCES [dbo].[markets] ([market_code]),
    CONSTRAINT [FK_station_inventory_spot_snapshots_proposal_version_detail_quarter_weeks] FOREIGN KEY ([proposal_version_detail_quarter_week_id]) REFERENCES [dbo].[proposal_version_detail_quarter_weeks] ([id]),
    CONSTRAINT [FK_station_inventory_spot_snapshots_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id]),
    CONSTRAINT [FK_station_inventory_spot_snapshots_stations] FOREIGN KEY ([station_id]) REFERENCES [dbo].[stations] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_spot_snapshots_stations]
    ON [dbo].[station_inventory_spot_snapshots]([station_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_spot_snapshots_markets]
    ON [dbo].[station_inventory_spot_snapshots]([station_market_code] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_spot_snapshots_spot_lengths]
    ON [dbo].[station_inventory_spot_snapshots]([spot_length_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_spot_snapshots_proposal_version_detail_quarter_weeks]
    ON [dbo].[station_inventory_spot_snapshots]([proposal_version_detail_quarter_week_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_spot_snapshots_dayparts]
    ON [dbo].[station_inventory_spot_snapshots]([daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_spot_snapshots_audiences]
    ON [dbo].[station_inventory_spot_snapshots]([audience_id] ASC);

