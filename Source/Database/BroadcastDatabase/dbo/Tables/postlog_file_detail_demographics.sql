CREATE TABLE [dbo].[postlog_file_detail_demographics] (
    [id]                     INT        IDENTITY (1, 1) NOT NULL,
    [audience_id]            INT        NULL,
    [postlog_file_detail_id] BIGINT     NULL,
    [overnight_rating]       FLOAT (53) NULL,
    [overnight_impressions]  FLOAT (53) NULL,
    CONSTRAINT [PK_postlog_file_detail_demographics] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_postlog_file_detail_demographics_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_postlog_file_detail_demographics_postlog_file_details] FOREIGN KEY ([postlog_file_detail_id]) REFERENCES [dbo].[postlog_file_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_postlog_file_detail_demographics_postlog_file_details]
    ON [dbo].[postlog_file_detail_demographics]([postlog_file_detail_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_postlog_file_detail_demographics_audiences]
    ON [dbo].[postlog_file_detail_demographics]([audience_id] ASC);

