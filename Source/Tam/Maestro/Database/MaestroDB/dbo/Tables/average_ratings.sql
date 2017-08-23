CREATE TABLE [dbo].[average_ratings] (
    [rating_category_id]      INT        NOT NULL,
    [base_media_month_id]     INT        NOT NULL,
    [forecast_media_month_id] INT        NOT NULL,
    [nielsen_network_id]      INT        NOT NULL,
    [audience_id]             INT        NOT NULL,
    [daypart_id]              INT        NOT NULL,
    [audience_usage]          FLOAT (53) NOT NULL,
    [tv_usage]                FLOAT (53) NOT NULL,
    CONSTRAINT [PK_average_ratings] PRIMARY KEY CLUSTERED ([rating_category_id] ASC, [base_media_month_id] ASC, [forecast_media_month_id] ASC, [nielsen_network_id] ASC, [audience_id] ASC, [daypart_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_average_ratings_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_average_ratings_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_average_ratings_media_month_forecast] FOREIGN KEY ([forecast_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_average_ratings_media_months_base] FOREIGN KEY ([base_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_average_ratings_nielsen_networks] FOREIGN KEY ([nielsen_network_id]) REFERENCES [dbo].[nielsen_networks] ([id]),
    CONSTRAINT [FK_average_ratings_rating_categories] FOREIGN KEY ([rating_category_id]) REFERENCES [dbo].[rating_categories] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'rating_category_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'rating_category_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'base_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'base_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'forecast_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'forecast_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'nielsen_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'nielsen_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'audience_usage';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'audience_usage';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'tv_usage';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'average_ratings', @level2type = N'COLUMN', @level2name = N'tv_usage';

