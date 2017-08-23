CREATE TABLE [dbo].[traffic_footprint] (
    [traffic_id] INT NOT NULL,
    [zone_id]    INT NOT NULL,
    CONSTRAINT [PK_traffic_footprint] PRIMARY KEY CLUSTERED ([traffic_id] ASC, [zone_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_footprint_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id]),
    CONSTRAINT [FK_traffic_footprint_zones] FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);

