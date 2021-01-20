CREATE TABLE [dbo].[station_inventory_loaded] (
    [id]                  INT      IDENTITY (1, 1) NOT NULL,
    [inventory_source_id] INT      NOT NULL,
    [last_loaded]         DATETIME NOT NULL,
    [station_id]          INT      NOT NULL,
    CONSTRAINT [PK_station_inventory_loaded] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_inventory_loaded_inventory_sources] FOREIGN KEY ([inventory_source_id]) REFERENCES [dbo].[inventory_sources] ([id]),
    CONSTRAINT [FK_station_inventory_loaded_stations] FOREIGN KEY ([station_id]) REFERENCES [dbo].[stations] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_loaded_stations]
    ON [dbo].[station_inventory_loaded]([station_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_loaded_inventory_sources]
    ON [dbo].[station_inventory_loaded]([inventory_source_id] ASC);

