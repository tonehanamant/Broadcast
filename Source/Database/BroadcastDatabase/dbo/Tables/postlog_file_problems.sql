CREATE TABLE [dbo].[postlog_file_problems] (
    [id]                  BIGINT         IDENTITY (1, 1) NOT NULL,
    [postlog_file_id]     INT            NOT NULL,
    [problem_description] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_postlog_file_problems] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_postlog_file_problems_postlog_files] FOREIGN KEY ([postlog_file_id]) REFERENCES [dbo].[postlog_files] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_postlog_file_problems_postlog_files]
    ON [dbo].[postlog_file_problems]([postlog_file_id] ASC);

