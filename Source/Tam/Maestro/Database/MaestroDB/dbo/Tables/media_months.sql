CREATE TABLE [dbo].[media_months] (
    [id]          INT          IDENTITY (1, 1) NOT NULL,
    [year]        INT          NOT NULL,
    [month]       INT          NOT NULL,
    [media_month] VARCHAR (15) NOT NULL,
    [start_date]  DATETIME     NOT NULL,
    [end_date]    DATETIME     NOT NULL,
    CONSTRAINT [PK_media_months] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [SK_media_months_1]
    ON [dbo].[media_months]([year] ASC, [month] ASC) WITH (FILLFACTOR = 90);


GO
CREATE UNIQUE NONCLUSTERED INDEX [SK_media_months_2]
    ON [dbo].[media_months]([media_month] ASC) WITH (FILLFACTOR = 90);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_media_months_dates]
    ON [dbo].[media_months]([start_date] ASC, [end_date] ASC) WITH (FILLFACTOR = 90);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains all of the media months for each year.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Media Months', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Use this table to relate an object to a media month.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The year the media month belongs to.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months', @level2type = N'COLUMN', @level2name = N'year';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months', @level2type = N'COLUMN', @level2name = N'year';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The month the media month belongs to.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months', @level2type = N'COLUMN', @level2name = N'month';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months', @level2type = N'COLUMN', @level2name = N'month';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The textual representation of the media month.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months', @level2type = N'COLUMN', @level2name = N'media_month';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months', @level2type = N'COLUMN', @level2name = N'media_month';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The start date of the media month.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'This should correspond with the start date of the first week in the media month.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The end date of the media month.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'This should correspond with the end date of the last week in the media month.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'media_months', @level2type = N'COLUMN', @level2name = N'end_date';

