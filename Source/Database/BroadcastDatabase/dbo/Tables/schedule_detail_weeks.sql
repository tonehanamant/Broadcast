CREATE TABLE [dbo].[schedule_detail_weeks] (
    [id]                 INT      IDENTITY (1, 1) NOT NULL,
    [schedule_detail_id] INT      NOT NULL,
    [media_week_id]      INT      NOT NULL,
    [start_date]         DATETIME NOT NULL,
    [end_date]           DATETIME NOT NULL,
    [spots]              INT      NOT NULL,
    [filled_spots]       SMALLINT NOT NULL,
    CONSTRAINT [PK_schedule_detail_weeks] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_schedule_detail_weeks_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_schedule_detail_weeks_schedule_details] FOREIGN KEY ([schedule_detail_id]) REFERENCES [dbo].[schedule_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_schedule_detail_weeks_media_weeks]
    ON [dbo].[schedule_detail_weeks]([media_week_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_schedule_detail_weeks_schedule_details]
    ON [dbo].[schedule_detail_weeks]([schedule_detail_id] ASC);

