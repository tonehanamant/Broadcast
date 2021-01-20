CREATE TABLE [dbo].[post_file_demos] (
    [post_file_id] INT NOT NULL,
    [demo]         INT NOT NULL,
    CONSTRAINT [PK_post_file_demos] PRIMARY KEY CLUSTERED ([post_file_id] ASC, [demo] ASC),
    CONSTRAINT [FK_post_file_demos_post_files] FOREIGN KEY ([post_file_id]) REFERENCES [dbo].[post_files] ([id]) ON DELETE CASCADE
);

