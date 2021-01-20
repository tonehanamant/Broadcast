CREATE TABLE [dbo].[station_inventory_manifest_generation] (
    [id]                            INT IDENTITY (1, 1) NOT NULL,
    [station_inventory_manifest_id] INT NOT NULL,
    [media_week_id]                 INT NOT NULL,
    CONSTRAINT [PK_station_inventory_manifest_generation] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_inventory_manifest_generation_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_generation_station_inventory_manifest] FOREIGN KEY ([station_inventory_manifest_id]) REFERENCES [dbo].[station_inventory_manifest] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_generation_station_inventory_manifest]
    ON [dbo].[station_inventory_manifest_generation]([station_inventory_manifest_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_generation_media_weeks]
    ON [dbo].[station_inventory_manifest_generation]([media_week_id] ASC);

