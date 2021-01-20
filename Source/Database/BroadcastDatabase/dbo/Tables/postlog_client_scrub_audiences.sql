CREATE TABLE [dbo].[postlog_client_scrub_audiences] (
    [postlog_client_scrub_id] INT        NOT NULL,
    [audience_id]             INT        NOT NULL,
    [impressions]             FLOAT (53) NOT NULL,
    CONSTRAINT [PK_postlog_client_scrub_audiences] PRIMARY KEY CLUSTERED ([postlog_client_scrub_id] ASC, [audience_id] ASC),
    CONSTRAINT [FK_postlog_client_scrub_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_postlog_client_scrub_audiences_postlog_client_scrubs] FOREIGN KEY ([postlog_client_scrub_id]) REFERENCES [dbo].[postlog_client_scrubs] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_postlog_client_scrub_audiences_audiences]
    ON [dbo].[postlog_client_scrub_audiences]([audience_id] ASC);

