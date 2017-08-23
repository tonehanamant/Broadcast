CREATE TABLE [dbo].[traffic_daypart_exclusion_mvpds] (
    [traffic_daypart_exclusion_id] INT NOT NULL,
    [mvpd_id]                      INT NOT NULL,
    CONSTRAINT [PK_traffic_daypart_exclusion_mvpds] PRIMARY KEY CLUSTERED ([traffic_daypart_exclusion_id] ASC, [mvpd_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_daypart_exclusion_mvpds_businesses] FOREIGN KEY ([mvpd_id]) REFERENCES [dbo].[businesses] ([id]),
    CONSTRAINT [FK_traffic_daypart_exclusion_mvpds_traffic_daypart_exclusions] FOREIGN KEY ([traffic_daypart_exclusion_id]) REFERENCES [dbo].[traffic_daypart_exclusions] ([id])
);

