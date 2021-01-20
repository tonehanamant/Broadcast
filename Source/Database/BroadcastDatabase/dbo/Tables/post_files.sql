CREATE TABLE [dbo].[post_files] (
    [id]              INT           IDENTITY (1, 1) NOT NULL,
    [file_name]       VARCHAR (255) NOT NULL,
    [equivalized]     BIT           NOT NULL,
    [posting_book_id] INT           NOT NULL,
    [upload_date]     DATETIME      NOT NULL,
    [modified_date]   DATETIME      NOT NULL,
    [playback_type]   TINYINT       NOT NULL,
    CONSTRAINT [PK_post_files] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_post_files_media_months] FOREIGN KEY ([posting_book_id]) REFERENCES [dbo].[media_months] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_post_files_media_months]
    ON [dbo].[post_files]([posting_book_id] ASC);

