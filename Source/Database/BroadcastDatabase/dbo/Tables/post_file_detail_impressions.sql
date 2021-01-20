CREATE TABLE [dbo].[post_file_detail_impressions] (
    [post_file_detail_id] INT        NOT NULL,
    [demo]                INT        NOT NULL,
    [impression]          FLOAT (53) NOT NULL,
    CONSTRAINT [PK_post_file_detail_impressions] PRIMARY KEY CLUSTERED ([post_file_detail_id] ASC, [demo] ASC),
    CONSTRAINT [FK_post_file_detail_impressions_audiences] FOREIGN KEY ([demo]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_post_file_details_impressions_post_file_details] FOREIGN KEY ([post_file_detail_id]) REFERENCES [dbo].[post_file_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_post_file_detail_impressions_audiences]
    ON [dbo].[post_file_detail_impressions]([demo] ASC);

