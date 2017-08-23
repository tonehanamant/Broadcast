CREATE TABLE [dbo].[autoloaded_rating_categories] (
    [rating_category_id] INT           NOT NULL,
    [watch_folder]       VARCHAR (100) NOT NULL,
    [filename_template]  VARCHAR (100) NOT NULL,
    [watch_frequency_id] INT           CONSTRAINT [DF_autoloaded_rating_categories_frequency_id] DEFAULT ((0)) NOT NULL
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'autoloaded_rating_categories';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'autoloaded_rating_categories';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'autoloaded_rating_categories';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'autoloaded_rating_categories', @level2type = N'COLUMN', @level2name = N'rating_category_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'autoloaded_rating_categories', @level2type = N'COLUMN', @level2name = N'rating_category_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'autoloaded_rating_categories', @level2type = N'COLUMN', @level2name = N'watch_folder';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'autoloaded_rating_categories', @level2type = N'COLUMN', @level2name = N'watch_folder';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'autoloaded_rating_categories', @level2type = N'COLUMN', @level2name = N'filename_template';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'autoloaded_rating_categories', @level2type = N'COLUMN', @level2name = N'filename_template';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'autoloaded_rating_categories', @level2type = N'COLUMN', @level2name = N'watch_frequency_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'autoloaded_rating_categories', @level2type = N'COLUMN', @level2name = N'watch_frequency_id';

