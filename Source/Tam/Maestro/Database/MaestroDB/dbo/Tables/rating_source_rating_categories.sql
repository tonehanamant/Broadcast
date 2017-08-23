CREATE TABLE [dbo].[rating_source_rating_categories] (
    [rating_source_id]   TINYINT NOT NULL,
    [rating_category_id] INT     NOT NULL,
    CONSTRAINT [PK_rating_source_rating_categories] PRIMARY KEY CLUSTERED ([rating_source_id] ASC, [rating_category_id] ASC),
    CONSTRAINT [FK_rating_source_rating_categories_rating_categories] FOREIGN KEY ([rating_category_id]) REFERENCES [dbo].[rating_categories] ([id]),
    CONSTRAINT [FK_rating_source_rating_categories_rating_sources] FOREIGN KEY ([rating_source_id]) REFERENCES [dbo].[rating_sources] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rating_source_rating_categories';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rating_source_rating_categories';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rating_source_rating_categories';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rating_source_rating_categories', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rating_source_rating_categories', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rating_source_rating_categories', @level2type = N'COLUMN', @level2name = N'rating_category_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rating_source_rating_categories', @level2type = N'COLUMN', @level2name = N'rating_category_id';

