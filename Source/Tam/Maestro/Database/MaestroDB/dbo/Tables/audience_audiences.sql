CREATE TABLE [dbo].[audience_audiences] (
    [rating_category_group_id] TINYINT NOT NULL,
    [custom_audience_id]       INT     NOT NULL,
    [rating_audience_id]       INT     NOT NULL,
    CONSTRAINT [PK_audience_audiences] PRIMARY KEY CLUSTERED ([rating_category_group_id] ASC, [custom_audience_id] ASC, [rating_audience_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_audience_audiences_audiences] FOREIGN KEY ([custom_audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_audience_audiences_audiences1] FOREIGN KEY ([rating_audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_audience_audiences_rating_category_groups] FOREIGN KEY ([rating_category_group_id]) REFERENCES [dbo].[rating_category_groups] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'audience_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'audience_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'audience_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'audience_audiences', @level2type = N'COLUMN', @level2name = N'rating_category_group_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'audience_audiences', @level2type = N'COLUMN', @level2name = N'rating_category_group_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'audience_audiences', @level2type = N'COLUMN', @level2name = N'custom_audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'audience_audiences', @level2type = N'COLUMN', @level2name = N'custom_audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'audience_audiences', @level2type = N'COLUMN', @level2name = N'rating_audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'audience_audiences', @level2type = N'COLUMN', @level2name = N'rating_audience_id';

