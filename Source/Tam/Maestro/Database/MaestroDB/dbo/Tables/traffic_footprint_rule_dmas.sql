CREATE TABLE [dbo].[traffic_footprint_rule_dmas] (
    [traffic_footprint_rule_id] INT NOT NULL,
    [dma_id]                    INT NOT NULL,
    CONSTRAINT [PK_traffic_footprint_rule_dmas] PRIMARY KEY CLUSTERED ([traffic_footprint_rule_id] ASC, [dma_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_footprint_rule_dmas_dmas] FOREIGN KEY ([dma_id]) REFERENCES [dbo].[dmas] ([id]),
    CONSTRAINT [FK_traffic_footprint_rule_dmas_traffic_footprint_rules] FOREIGN KEY ([traffic_footprint_rule_id]) REFERENCES [dbo].[traffic_footprint_rules] ([id])
);

