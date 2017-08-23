CREATE TABLE [dbo].[traffic_index_adjustment_networks] (
    [traffic_index_adjustment_id] INT NOT NULL,
    [network_id]                  INT NOT NULL,
    CONSTRAINT [PK_traffic_index_adjustment_networks] PRIMARY KEY CLUSTERED ([traffic_index_adjustment_id] ASC, [network_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_index_adjustment_networks_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_traffic_index_adjustment_networks_traffic_index_adjustments] FOREIGN KEY ([traffic_index_adjustment_id]) REFERENCES [dbo].[traffic_index_adjustments] ([id])
);

