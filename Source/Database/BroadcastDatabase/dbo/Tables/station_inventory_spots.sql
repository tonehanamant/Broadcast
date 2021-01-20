CREATE TABLE [dbo].[station_inventory_spots] (
    [id]                                      INT             IDENTITY (1, 1) NOT NULL,
    [proposal_version_detail_quarter_week_id] INT             NULL,
    [station_inventory_manifest_id]           INT             NOT NULL,
    [media_week_id]                           INT             NOT NULL,
    [inventory_lost]                          BIT             NOT NULL,
    [overridden_impressions]                  FLOAT (53)      NULL,
    [overridden_rate]                         DECIMAL (19, 4) NULL,
    [delivery_cpm]                            DECIMAL (19, 4) NULL,
    CONSTRAINT [PK_station_inventory_spots] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_inventory_spots_proposal_version_detail_quarter_weeks] FOREIGN KEY ([proposal_version_detail_quarter_week_id]) REFERENCES [dbo].[proposal_version_detail_quarter_weeks] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_station_inventory_spots_station_inventory_manifest] FOREIGN KEY ([station_inventory_manifest_id]) REFERENCES [dbo].[station_inventory_manifest] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_spots_station_inventory_manifest]
    ON [dbo].[station_inventory_spots]([station_inventory_manifest_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_spots_proposal_version_detail_quarter_weeks]
    ON [dbo].[station_inventory_spots]([proposal_version_detail_quarter_week_id] ASC);

