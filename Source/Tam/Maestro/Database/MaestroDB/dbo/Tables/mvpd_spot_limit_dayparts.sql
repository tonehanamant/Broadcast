CREATE TABLE [dbo].[mvpd_spot_limit_dayparts] (
    [mvpd_spot_limit_id] INT NOT NULL,
    [daypart_id]         INT NOT NULL,
    [spot_limit]         INT NOT NULL,
    CONSTRAINT [PK_mvpd_spot_limit_dayparts] PRIMARY KEY CLUSTERED ([mvpd_spot_limit_id] ASC, [daypart_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_mvpd_spot_limit_dayparts_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_mvpd_spot_limit_dayparts_mvpd_spot_limits] FOREIGN KEY ([mvpd_spot_limit_id]) REFERENCES [dbo].[mvpd_spot_limits] ([id])
);

