CREATE TABLE [dbo].[ratings_out] (
    [rating_category_id]      INT        NOT NULL,
    [base_media_month_id]     INT        NOT NULL,
    [forecast_media_month_id] INT        NOT NULL,
    [nielsen_network_id]      INT        NOT NULL,
    [audience_id]             INT        NOT NULL,
    [daypart_id]              INT        NOT NULL,
    [audience_usage]          FLOAT (53) NOT NULL,
    [tv_usage]                FLOAT (53) NOT NULL,
    CONSTRAINT [PK_ratings_out] PRIMARY KEY CLUSTERED ([rating_category_id] ASC, [base_media_month_id] ASC, [forecast_media_month_id] ASC, [nielsen_network_id] ASC, [audience_id] ASC, [daypart_id] ASC) ON [MediaMonthIDPScheme] ([base_media_month_id]),
    CONSTRAINT [FK_ratings_out_base_media_months] FOREIGN KEY ([base_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_ratings_out_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_ratings_out_forecast_media_month] FOREIGN KEY ([forecast_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_ratings_out_nielsen_networks] FOREIGN KEY ([nielsen_network_id]) REFERENCES [dbo].[nielsen_networks] ([id]),
    CONSTRAINT [FK_ratings_out_rating_categories] FOREIGN KEY ([rating_category_id]) REFERENCES [dbo].[rating_categories] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'rating_category_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'rating_category_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'base_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'base_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'forecast_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'forecast_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'nielsen_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'nielsen_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'audience_usage';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'audience_usage';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'tv_usage';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ratings_out', @level2type = N'COLUMN', @level2name = N'tv_usage';

