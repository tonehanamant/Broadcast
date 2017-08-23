CREATE TABLE [dbo].[mvpd_spot_limits] (
    [id]                  INT IDENTITY (1, 1) NOT NULL,
    [mvpd_id]             INT NOT NULL,
    [start_media_week_id] INT NOT NULL,
    [end_media_week_id]   INT NULL,
    CONSTRAINT [PK_mvpd_spot_limits] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_mvpd_spot_limits_media_weeks] FOREIGN KEY ([start_media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_mvpd_spot_limits_media_weeks1] FOREIGN KEY ([end_media_week_id]) REFERENCES [dbo].[media_weeks] ([id])
);

