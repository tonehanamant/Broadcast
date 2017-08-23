CREATE TABLE [dbo].[traffic_cpm_adjustment_mvpds] (
    [traffic_cpm_adjustment_id] INT NOT NULL,
    [mvpd_id]                   INT NOT NULL,
    CONSTRAINT [PK_traffic_cpm_adjustment_mvpds] PRIMARY KEY CLUSTERED ([traffic_cpm_adjustment_id] ASC, [mvpd_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_cpm_adjustment_mvpds_businesses] FOREIGN KEY ([mvpd_id]) REFERENCES [dbo].[businesses] ([id]),
    CONSTRAINT [FK_traffic_cpm_adjustment_mvpds_traffic_cpm_adjustments] FOREIGN KEY ([traffic_cpm_adjustment_id]) REFERENCES [dbo].[traffic_cpm_adjustments] ([id])
);

