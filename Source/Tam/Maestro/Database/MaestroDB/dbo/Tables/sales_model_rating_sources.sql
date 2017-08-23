CREATE TABLE [dbo].[sales_model_rating_sources] (
    [sales_model_id]     INT     NOT NULL,
    [rating_source_id]   TINYINT NOT NULL,
    [is_default]         BIT     NOT NULL,
    [use_for_posts]      BIT     CONSTRAINT [DF_sales_model_rating_sources_use_for_posts] DEFAULT ((1)) NOT NULL,
    [use_for_proposals]  BIT     CONSTRAINT [DF_sales_model_rating_sources_use_for_proposals] DEFAULT ((1)) NOT NULL,
    [use_for_rate_cards] BIT     CONSTRAINT [DF_sales_model_rating_sources_use_for_rate_cards] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_sales_model_rating_sources] PRIMARY KEY CLUSTERED ([sales_model_id] ASC, [rating_source_id] ASC),
    CONSTRAINT [FK_sales_model_rating_sources_rating_sources] FOREIGN KEY ([rating_source_id]) REFERENCES [dbo].[rating_sources] ([id]),
    CONSTRAINT [FK_sales_model_rating_sources_sales_models] FOREIGN KEY ([sales_model_id]) REFERENCES [dbo].[sales_models] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources', @level2type = N'COLUMN', @level2name = N'sales_model_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources', @level2type = N'COLUMN', @level2name = N'sales_model_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources', @level2type = N'COLUMN', @level2name = N'is_default';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources', @level2type = N'COLUMN', @level2name = N'is_default';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources', @level2type = N'COLUMN', @level2name = N'use_for_posts';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources', @level2type = N'COLUMN', @level2name = N'use_for_posts';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources', @level2type = N'COLUMN', @level2name = N'use_for_proposals';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources', @level2type = N'COLUMN', @level2name = N'use_for_proposals';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources', @level2type = N'COLUMN', @level2name = N'use_for_rate_cards';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_rating_sources', @level2type = N'COLUMN', @level2name = N'use_for_rate_cards';

