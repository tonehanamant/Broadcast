CREATE TABLE [dbo].[nti_to_nsi_conversion_rates] (
    [id]                  INT        IDENTITY (1, 1) NOT NULL,
    [conversion_rate]     FLOAT (53) NOT NULL,
    [media_month_id]      INT        NOT NULL,
    [standard_daypart_id] INT        NOT NULL,
    CONSTRAINT [PK_nti_to_nsi_conversion_rates] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_nti_to_nsi_conversion_rates_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_nti_to_nsi_conversion_rates_standard_dayparts] FOREIGN KEY ([standard_daypart_id]) REFERENCES [dbo].[standard_dayparts] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_nti_to_nsi_conversion_rates_standard_dayparts]
    ON [dbo].[nti_to_nsi_conversion_rates]([standard_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_nti_to_nsi_conversion_rates_media_months]
    ON [dbo].[nti_to_nsi_conversion_rates]([media_month_id] ASC);

