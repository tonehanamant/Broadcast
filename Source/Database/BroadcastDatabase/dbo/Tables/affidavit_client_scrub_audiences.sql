CREATE TABLE [dbo].[affidavit_client_scrub_audiences] (
    [affidavit_client_scrub_id] INT        NOT NULL,
    [audience_id]               INT        NOT NULL,
    [impressions]               FLOAT (53) NOT NULL,
    CONSTRAINT [PK_affidavit_client_scrub_audiences] PRIMARY KEY CLUSTERED ([affidavit_client_scrub_id] ASC, [audience_id] ASC),
    CONSTRAINT [FK_affidavit_client_scrub_audiences_affidavit_client_scrubs] FOREIGN KEY ([affidavit_client_scrub_id]) REFERENCES [dbo].[affidavit_client_scrubs] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_affidavit_client_scrub_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_affidavit_client_scrub_audiences_audiences]
    ON [dbo].[affidavit_client_scrub_audiences]([audience_id] ASC);

