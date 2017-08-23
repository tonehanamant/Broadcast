CREATE TABLE [dbo].[traffic_cpm_adjustment_networks] (
    [traffic_cpm_adjustment_id] INT   NOT NULL,
    [network_id]                INT   NOT NULL,
    [cpm]                       MONEY NOT NULL,
    [proposed_cpm]              MONEY NOT NULL,
    [traffic_detail_id]         INT   NOT NULL,
    CONSTRAINT [PK_traffic_cpm_adjustment_networks] PRIMARY KEY CLUSTERED ([traffic_cpm_adjustment_id] ASC, [traffic_detail_id] ASC),
    CONSTRAINT [FK_traffic_cpm_adjustment_networks_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_traffic_cpm_adjustment_networks_traffic_cpm_adjustments] FOREIGN KEY ([traffic_cpm_adjustment_id]) REFERENCES [dbo].[traffic_cpm_adjustments] ([id])
);

