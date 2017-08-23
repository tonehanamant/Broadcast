CREATE TABLE [dbo].[traffic_footprint_rules] (
    [id]          INT IDENTITY (1, 1) NOT NULL,
    [traffic_id]  INT NOT NULL,
    [is_included] BIT NOT NULL,
    CONSTRAINT [PK_traffic_footprint_rules] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_footprint_rules_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id])
);

