CREATE TABLE [dbo].[forecasted_media_months] (
    [base_media_month_id]           INT NOT NULL,
    [rating_category_id]            INT NOT NULL,
    [start_forecast_media_month_id] INT NOT NULL,
    [end_forecast_media_month_id]   INT NOT NULL,
    [is_forecasted]                 BIT NOT NULL,
    CONSTRAINT [PK_forecasted_media_months] PRIMARY KEY CLUSTERED ([base_media_month_id] ASC, [rating_category_id] ASC),
    CONSTRAINT [FK_forecasted_media_months_media_months] FOREIGN KEY ([base_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_forecasted_media_months_media_months1] FOREIGN KEY ([start_forecast_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_forecasted_media_months_media_months2] FOREIGN KEY ([end_forecast_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_forecasted_media_months_rating_categories] FOREIGN KEY ([rating_category_id]) REFERENCES [dbo].[rating_categories] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'forecasted_media_months';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'forecasted_media_months';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'forecasted_media_months';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'forecasted_media_months', @level2type = N'COLUMN', @level2name = N'base_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'forecasted_media_months', @level2type = N'COLUMN', @level2name = N'base_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'forecasted_media_months', @level2type = N'COLUMN', @level2name = N'rating_category_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'forecasted_media_months', @level2type = N'COLUMN', @level2name = N'rating_category_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'forecasted_media_months', @level2type = N'COLUMN', @level2name = N'start_forecast_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'forecasted_media_months', @level2type = N'COLUMN', @level2name = N'start_forecast_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'forecasted_media_months', @level2type = N'COLUMN', @level2name = N'end_forecast_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'forecasted_media_months', @level2type = N'COLUMN', @level2name = N'end_forecast_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'forecasted_media_months', @level2type = N'COLUMN', @level2name = N'is_forecasted';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'forecasted_media_months', @level2type = N'COLUMN', @level2name = N'is_forecasted';

