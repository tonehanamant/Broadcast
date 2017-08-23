CREATE TABLE [dbo].[traffic_cpm_adjustment_dmas] (
    [traffic_cpm_adjustment_id] INT NOT NULL,
    [dma_id]                    INT NOT NULL,
    CONSTRAINT [PK_traffic_cpm_adjustment_dmas] PRIMARY KEY CLUSTERED ([traffic_cpm_adjustment_id] ASC, [dma_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_cpm_adjustment_dmas_dmas] FOREIGN KEY ([dma_id]) REFERENCES [dbo].[dmas] ([id]),
    CONSTRAINT [FK_traffic_cpm_adjustment_dmas_traffic_cpm_adjustments] FOREIGN KEY ([traffic_cpm_adjustment_id]) REFERENCES [dbo].[traffic_cpm_adjustments] ([id])
);

