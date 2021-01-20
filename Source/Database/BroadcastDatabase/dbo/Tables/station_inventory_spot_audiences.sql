CREATE TABLE [dbo].[station_inventory_spot_audiences] (
    [station_inventory_spot_id] INT             NOT NULL,
    [audience_id]               INT             NOT NULL,
    [calculated_impressions]    FLOAT (53)      NULL,
    [calculated_rate]           DECIMAL (19, 4) NULL,
    CONSTRAINT [PK_station_inventory_spot_audiences] PRIMARY KEY CLUSTERED ([station_inventory_spot_id] ASC, [audience_id] ASC),
    CONSTRAINT [FK_station_inventory_spot_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_station_inventory_spot_audiences_station_inventory_spots] FOREIGN KEY ([station_inventory_spot_id]) REFERENCES [dbo].[station_inventory_spots] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_spot_audiences_audiences]
    ON [dbo].[station_inventory_spot_audiences]([audience_id] ASC);

