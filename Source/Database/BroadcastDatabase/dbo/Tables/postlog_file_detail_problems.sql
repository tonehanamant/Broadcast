CREATE TABLE [dbo].[postlog_file_detail_problems] (
    [id]                     INT           IDENTITY (1, 1) NOT NULL,
    [postlog_file_detail_id] BIGINT        NOT NULL,
    [problem_type]           INT           NOT NULL,
    [problem_description]    VARCHAR (255) NULL,
    CONSTRAINT [PK_postlog_file_detail_problems] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_postlog_file_detail_problems_postlog_file_details] FOREIGN KEY ([postlog_file_detail_id]) REFERENCES [dbo].[postlog_file_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_postlog_file_detail_problems_postlog_file_details]
    ON [dbo].[postlog_file_detail_problems]([postlog_file_detail_id] ASC);

