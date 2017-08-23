CREATE TABLE [dbo].[traffic_spot_cap] (
    [id]                  INT        IDENTITY (1, 1) NOT NULL,
    [daypart_id]          INT        NOT NULL,
    [spots_per_hour]      FLOAT (53) NOT NULL,
    [start_media_week_id] INT        NOT NULL,
    [end_media_week_id]   INT        NULL,
    CONSTRAINT [PK_traffic_spot_cap] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_traffic_spot_cap_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_traffic_spot_cap_media_weeks] FOREIGN KEY ([start_media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_traffic_spot_cap_media_weeks1] FOREIGN KEY ([end_media_week_id]) REFERENCES [dbo].[media_weeks] ([id])
);

