CREATE TABLE [dbo].[station_inventory_manifest_weeks] (
    [id]                            INT      IDENTITY (1, 1) NOT NULL,
    [station_inventory_manifest_id] INT      NOT NULL,
    [media_week_id]                 INT      NOT NULL,
    [spots]                         INT      NOT NULL,
    [start_date]                    DATETIME NOT NULL,
    [end_date]                      DATETIME NOT NULL,
    [sys_start_date]                DATETIME NOT NULL,
    [sys_end_date]                  DATETIME NOT NULL,
    CONSTRAINT [PK_station_inventory_manifest_weeks] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_inventory_manifest_weeks_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_weeks_station_inventory_manifest] FOREIGN KEY ([station_inventory_manifest_id]) REFERENCES [dbo].[station_inventory_manifest] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_weeks_station_inventory_manifest]
    ON [dbo].[station_inventory_manifest_weeks]([station_inventory_manifest_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_weeks_media_weeks]
    ON [dbo].[station_inventory_manifest_weeks]([media_week_id] ASC);

