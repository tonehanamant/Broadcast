CREATE TABLE [dbo].[tam_post_dayparts] (
    [tam_post_id] INT     NOT NULL,
    [daypart_id]  INT     NOT NULL,
    [ordinal]     TINYINT NOT NULL,
    CONSTRAINT [PK_tam_post_dayparts] PRIMARY KEY CLUSTERED ([tam_post_id] ASC, [daypart_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_tam_post_dayparts_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_tam_post_dayparts_tam_posts] FOREIGN KEY ([tam_post_id]) REFERENCES [dbo].[tam_posts] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains a custom set of dayparts to use for the daypart post analysis report.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dayparts';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'TAM Post Dayparts', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dayparts';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dayparts';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dayparts', @level2type = N'COLUMN', @level2name = N'tam_post_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dayparts', @level2type = N'COLUMN', @level2name = N'tam_post_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the daypart.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dayparts', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dayparts', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The order to display the dayparts.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dayparts', @level2type = N'COLUMN', @level2name = N'ordinal';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dayparts', @level2type = N'COLUMN', @level2name = N'ordinal';

