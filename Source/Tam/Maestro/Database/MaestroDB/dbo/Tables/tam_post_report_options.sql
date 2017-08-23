CREATE TABLE [dbo].[tam_post_report_options] (
    [tam_post_id]                   INT     NOT NULL,
    [post_buy_analysis_report_code] TINYINT NOT NULL,
    CONSTRAINT [PK_tam_post_report_options] PRIMARY KEY CLUSTERED ([tam_post_id] ASC, [post_buy_analysis_report_code] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_tam_post_report_options_tam_posts] FOREIGN KEY ([tam_post_id]) REFERENCES [dbo].[tam_posts] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains the default report options for a post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_report_options';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'TAM Post Report Options', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_report_options';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Each report option represents a different type of page in a post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_report_options';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_report_options', @level2type = N'COLUMN', @level2name = N'tam_post_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_report_options', @level2type = N'COLUMN', @level2name = N'tam_post_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The report code specifies the type of page to be printed.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_report_options', @level2type = N'COLUMN', @level2name = N'post_buy_analysis_report_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Too many codes to list.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_report_options', @level2type = N'COLUMN', @level2name = N'post_buy_analysis_report_code';

