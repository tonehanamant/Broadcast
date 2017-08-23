CREATE TABLE [dbo].[traffic_flight_weeks] (
    [traffic_id]      INT        NOT NULL,
    [start_date]      DATE       NOT NULL,
    [end_date]        DATE       NOT NULL,
    [media_week_id]   INT        NOT NULL,
    [is_goal_week]    BIT        NOT NULL,
    [unit_goal]       INT        NOT NULL,
    [impression_goal] FLOAT (53) NOT NULL,
    [is_traffic_week] BIT        NOT NULL,
    CONSTRAINT [PK_traffic_flight_weeks] PRIMARY KEY CLUSTERED ([traffic_id] ASC, [start_date] ASC, [end_date] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_flight_weeks_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_traffic_flight_weeks_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id])
);

