CREATE TABLE [dbo].[station_inventory_manifest] (
    [id]                         INT           IDENTITY (1, 1) NOT NULL,
    [spot_length_id]             INT           NOT NULL,
    [spots_per_week]             INT           NULL,
    [inventory_source_id]        INT           NOT NULL,
    [station_inventory_group_id] INT           NULL,
    [file_id]                    INT           NULL,
    [spots_per_day]              INT           NULL,
    [comment]                    VARCHAR (255) NULL,
    [station_id]                 INT           NULL,
    CONSTRAINT [PK_station_inventory_manifest] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_inventory_manifest_inventory_files] FOREIGN KEY ([file_id]) REFERENCES [dbo].[inventory_files] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_inventory_sources] FOREIGN KEY ([inventory_source_id]) REFERENCES [dbo].[inventory_sources] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_station_inventory_group] FOREIGN KEY ([station_inventory_group_id]) REFERENCES [dbo].[station_inventory_group] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_station_inventory_manifest_stations] FOREIGN KEY ([station_id]) REFERENCES [dbo].[stations] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_stations]
    ON [dbo].[station_inventory_manifest]([station_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_inventory_files]
    ON [dbo].[station_inventory_manifest]([file_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_station_inventory_group]
    ON [dbo].[station_inventory_manifest]([station_inventory_group_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_spot_lengths]
    ON [dbo].[station_inventory_manifest]([spot_length_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_inventory_sources]
    ON [dbo].[station_inventory_manifest]([inventory_source_id] ASC);

