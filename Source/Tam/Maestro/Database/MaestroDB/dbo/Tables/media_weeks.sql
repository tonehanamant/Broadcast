CREATE TABLE [dbo].[media_weeks] (
    [id]             INT      IDENTITY (1, 1) NOT NULL,
    [media_month_id] INT      NOT NULL,
    [week_number]    INT      NOT NULL,
    [start_date]     DATETIME NOT NULL,
    [end_date]       DATETIME NOT NULL,
    CONSTRAINT [PK_media_weeks] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_media_weeks_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [SK_media_weeks]
    ON [dbo].[media_weeks]([media_month_id] ASC, [week_number] ASC) WITH (FILLFACTOR = 90);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_media_weeks_dates]
    ON [dbo].[media_weeks]([start_date] ASC, [end_date] ASC) WITH (FILLFACTOR = 90);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains all the media weeks for each media month.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_weeks';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Media Month Weeks', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_weeks';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Use this table to officially lookup what weeks are in a given media month.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_weeks';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_weeks', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_weeks', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the media month this media week belongs to.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_weeks', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_weeks', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The number of the week in the media month.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_weeks', @level2type = N'COLUMN', @level2name = N'week_number';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Sorting by this lowest to highest for a given media month will give you the proper order of the weeks.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_weeks', @level2type = N'COLUMN', @level2name = N'week_number';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Start date of the media week.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_weeks', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_weeks', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'End date of the media week.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_weeks', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_weeks', @level2type = N'COLUMN', @level2name = N'end_date';

