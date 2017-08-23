CREATE TABLE [dbo].[traffic_index_adjustment_dayparts] (
    [traffic_index_adjustment_id] INT NOT NULL,
    [daypart_id]                  INT NOT NULL,
    CONSTRAINT [PK_traffic_index_adjustment_dayparts] PRIMARY KEY CLUSTERED ([traffic_index_adjustment_id] ASC, [daypart_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_index_adjustment_dayparts_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_traffic_index_adjustment_dayparts_traffic_index_adjustments] FOREIGN KEY ([traffic_index_adjustment_id]) REFERENCES [dbo].[traffic_index_adjustments] ([id])
);

