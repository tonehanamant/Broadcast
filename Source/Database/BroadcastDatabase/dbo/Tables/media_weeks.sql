CREATE TABLE [dbo].[media_weeks] (
    [id]             INT      IDENTITY (1, 1) NOT NULL,
    [media_month_id] INT      NOT NULL,
    [week_number]    INT      NOT NULL,
    [start_date]     DATETIME NOT NULL,
    [end_date]       DATETIME NOT NULL,
    CONSTRAINT [PK_media_weeks] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_media_weeks_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_media_weeks_media_months]
    ON [dbo].[media_weeks]([media_month_id] ASC);

