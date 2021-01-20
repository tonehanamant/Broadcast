CREATE TABLE [dbo].[station_inventory_manifest_audiences] (
    [station_inventory_manifest_id] INT             NOT NULL,
    [audience_id]                   INT             NOT NULL,
    [impressions]                   FLOAT (53)      NULL,
    [id]                            INT             IDENTITY (1, 1) NOT NULL,
    [rating]                        FLOAT (53)      NULL,
    [is_reference]                  BIT             NOT NULL,
    [cpm]                           DECIMAL (19, 4) NULL,
    [vpvh]                          FLOAT (53)      NULL,
    [share_playback_type]           INT             NULL,
    [hut_playback_type]             INT             NULL,
    CONSTRAINT [PK_station_inventory_manifest_audiences] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_inventory_manifest_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_audiences_station_inventory_manifest] FOREIGN KEY ([station_inventory_manifest_id]) REFERENCES [dbo].[station_inventory_manifest] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_audiences_station_inventory_manifest]
    ON [dbo].[station_inventory_manifest_audiences]([station_inventory_manifest_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_audiences_audiences]
    ON [dbo].[station_inventory_manifest_audiences]([audience_id] ASC);

