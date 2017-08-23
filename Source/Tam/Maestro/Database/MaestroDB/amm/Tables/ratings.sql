CREATE TABLE [amm].[ratings] (
    [id]                      INT        IDENTITY (1, 1) NOT NULL,
    [base_media_month_id]     INT        NOT NULL,
    [nielsen_network_id]      INT        NOT NULL,
    [forecast_media_month_id] INT        NOT NULL,
    [daypart_id]              INT        NOT NULL,
    [audience_id]             INT        NOT NULL,
    [audience_usage]          FLOAT (53) NOT NULL,
    [tv_usage]                FLOAT (53) NOT NULL,
    CONSTRAINT [PK_amm_rating] PRIMARY KEY CLUSTERED ([id] ASC, [base_media_month_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_amm_rating_2_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_amm_rating_2_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_amm_rating_2_media_months] FOREIGN KEY ([base_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_amm_rating_2_nielsen_networks] FOREIGN KEY ([nielsen_network_id]) REFERENCES [dbo].[nielsen_networks] ([id]),
    CONSTRAINT [FK_amm_ratings_forecast_media_month] FOREIGN KEY ([forecast_media_month_id]) REFERENCES [dbo].[media_months] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'base_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'base_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'nielsen_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'nielsen_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'forecast_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'forecast_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'audience_usage';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'audience_usage';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'tv_usage';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'amm', @level1type = N'TABLE', @level1name = N'ratings', @level2type = N'COLUMN', @level2name = N'tv_usage';

