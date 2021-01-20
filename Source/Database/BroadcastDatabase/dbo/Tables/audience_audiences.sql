CREATE TABLE [dbo].[audience_audiences] (
    [rating_category_group_id] TINYINT NOT NULL,
    [custom_audience_id]       INT     NOT NULL,
    [rating_audience_id]       INT     NOT NULL,
    CONSTRAINT [PK_audience_audiences] PRIMARY KEY CLUSTERED ([rating_category_group_id] ASC, [custom_audience_id] ASC, [rating_audience_id] ASC),
    CONSTRAINT [FK_audience_audiences_audiences] FOREIGN KEY ([custom_audience_id]) REFERENCES [dbo].[audiences] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_audience_audiences_audiences]
    ON [dbo].[audience_audiences]([custom_audience_id] ASC);

