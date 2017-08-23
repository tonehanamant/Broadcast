CREATE TABLE [dbo].[traffic_index_adjustment_weeks] (
    [traffic_index_adjustment_id] INT NOT NULL,
    [media_week_id]               INT NOT NULL,
    CONSTRAINT [PK_traffic_index_adjustment_weeks] PRIMARY KEY CLUSTERED ([traffic_index_adjustment_id] ASC, [media_week_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_index_adjustment_weeks_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_traffic_index_adjustment_weeks_traffic_index_adjustments] FOREIGN KEY ([traffic_index_adjustment_id]) REFERENCES [dbo].[traffic_index_adjustments] ([id])
);

