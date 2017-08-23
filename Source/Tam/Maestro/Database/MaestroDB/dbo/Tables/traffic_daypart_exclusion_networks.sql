CREATE TABLE [dbo].[traffic_daypart_exclusion_networks] (
    [traffic_daypart_exclusion_id] INT NOT NULL,
    [network_id]                   INT NOT NULL,
    CONSTRAINT [PK_traffic_daypart_exclusion_networks] PRIMARY KEY CLUSTERED ([traffic_daypart_exclusion_id] ASC, [network_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_daypart_exclusion_networks_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_traffic_daypart_exclusion_networks_traffic_daypart_exclusions] FOREIGN KEY ([traffic_daypart_exclusion_id]) REFERENCES [dbo].[traffic_daypart_exclusions] ([id])
);

