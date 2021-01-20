CREATE TABLE [dbo].[station_inventory_manifest_daypart_genres] (
    [id]                                    INT IDENTITY (1, 1) NOT NULL,
    [station_inventory_manifest_daypart_id] INT NOT NULL,
    [genre_id]                              INT NOT NULL,
    CONSTRAINT [PK_station_inventory_manifest_daypart_genres] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_inventory_manifest_daypart_genres_genres] FOREIGN KEY ([genre_id]) REFERENCES [dbo].[genres] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_daypart_genres_station_inventory_manifest_dayparts] FOREIGN KEY ([station_inventory_manifest_daypart_id]) REFERENCES [dbo].[station_inventory_manifest_dayparts] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_daypart_genres_genres]
    ON [dbo].[station_inventory_manifest_daypart_genres]([genre_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_daypart_genres_station_inventory_manifest_dayparts]
    ON [dbo].[station_inventory_manifest_daypart_genres]([station_inventory_manifest_daypart_id] ASC);

