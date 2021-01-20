CREATE TABLE [dbo].[station_inventory_manifest_rates] (
    [id]                            INT             IDENTITY (1, 1) NOT NULL,
    [station_inventory_manifest_id] INT             NOT NULL,
    [spot_length_id]                INT             NOT NULL,
    [spot_cost]                     DECIMAL (19, 4) NOT NULL,
    CONSTRAINT [PK_station_inventory_manifest_rates] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_inventory_manifest_rates_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_rates_station_inventory_manifest] FOREIGN KEY ([station_inventory_manifest_id]) REFERENCES [dbo].[station_inventory_manifest] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_rates_station_inventory_manifest]
    ON [dbo].[station_inventory_manifest_rates]([station_inventory_manifest_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_rates_spot_lengths]
    ON [dbo].[station_inventory_manifest_rates]([spot_length_id] ASC);

