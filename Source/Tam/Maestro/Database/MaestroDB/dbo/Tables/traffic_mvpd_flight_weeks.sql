CREATE TABLE [dbo].[traffic_mvpd_flight_weeks] (
    [traffic_id]       INT  NOT NULL,
    [mvpd_business_id] INT  NOT NULL,
    [start_date]       DATE NOT NULL,
    [end_date]         DATE NOT NULL,
    [media_week_id]    INT  NOT NULL,
    [is_goal_week]     BIT  NOT NULL,
    [is_traffic_week]  BIT  NOT NULL,
    CONSTRAINT [PK_traffic_mvpd_flight_weeks] PRIMARY KEY CLUSTERED ([traffic_id] ASC, [mvpd_business_id] ASC, [start_date] ASC, [end_date] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_mvpd_flight_weeks_businesses] FOREIGN KEY ([mvpd_business_id]) REFERENCES [dbo].[businesses] ([id]),
    CONSTRAINT [FK_traffic_mvpd_flight_weeks_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_traffic_mvpd_flight_weeks_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id])
);

