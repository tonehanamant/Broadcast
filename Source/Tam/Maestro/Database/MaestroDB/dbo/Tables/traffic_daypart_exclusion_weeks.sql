CREATE TABLE [dbo].[traffic_daypart_exclusion_weeks] (
    [traffic_daypart_exclusion_id] INT NOT NULL,
    [media_week_id]                INT NOT NULL,
    CONSTRAINT [PK_traffic_daypart_exclusion_weeks] PRIMARY KEY CLUSTERED ([traffic_daypart_exclusion_id] ASC, [media_week_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_daypart_exclusion_weeks_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_traffic_daypart_exclusion_weeks_traffic_daypart_exclusions] FOREIGN KEY ([traffic_daypart_exclusion_id]) REFERENCES [dbo].[traffic_daypart_exclusions] ([id])
);

