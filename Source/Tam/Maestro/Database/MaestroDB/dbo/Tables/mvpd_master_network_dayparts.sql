CREATE TABLE [dbo].[mvpd_master_network_dayparts] (
    [id]                  INT   IDENTITY (1, 1) NOT NULL,
    [mvpd_id]             INT   NOT NULL,
    [network_id]          INT   NULL,
    [daypart_id]          INT   NOT NULL,
    [start_media_week_id] INT   NOT NULL,
    [end_media_week_id]   INT   NULL,
    [can_be_altered]      BIT   NOT NULL,
    [minimum_spot_cost]   MONEY NULL,
    [spot_limit_count]    INT   NULL,
    CONSTRAINT [PK_mvpd_master_network_dayparts] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_mvpd_master_network_dayparts_businesses] FOREIGN KEY ([mvpd_id]) REFERENCES [dbo].[businesses] ([id]),
    CONSTRAINT [FK_mvpd_master_network_dayparts_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_mvpd_master_network_dayparts_media_weeks1] FOREIGN KEY ([start_media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_mvpd_master_network_dayparts_media_weeks2] FOREIGN KEY ([end_media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_mvpd_master_network_dayparts_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [UQ_mvpd_master_network_dayparts] UNIQUE NONCLUSTERED ([mvpd_id] ASC, [network_id] ASC, [daypart_id] ASC, [start_media_week_id] ASC)
);

