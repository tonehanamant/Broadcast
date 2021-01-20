CREATE TABLE [dbo].[station_inventory_manifest_genres] (
    [id]                            INT IDENTITY (1, 1) NOT NULL,
    [station_inventory_manifest_id] INT NOT NULL,
    [genre_id]                      INT NOT NULL,
    CONSTRAINT [PK_station_inventory_manifest_genres] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_inventory_manifest_genres_genres] FOREIGN KEY ([genre_id]) REFERENCES [dbo].[genres] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_genres_station_inventory_manifest] FOREIGN KEY ([station_inventory_manifest_id]) REFERENCES [dbo].[station_inventory_manifest] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_genres_genres]
    ON [dbo].[station_inventory_manifest_genres]([genre_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_genres_station_inventory_manifest]
    ON [dbo].[station_inventory_manifest_genres]([station_inventory_manifest_id] ASC);

