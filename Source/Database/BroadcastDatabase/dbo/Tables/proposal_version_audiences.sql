CREATE TABLE [dbo].[proposal_version_audiences] (
    [id]                  INT     IDENTITY (1, 1) NOT NULL,
    [proposal_version_id] INT     NOT NULL,
    [audience_id]         INT     NOT NULL,
    [rank]                TINYINT NOT NULL,
    CONSTRAINT [PK_proposal_version_audiences] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_version_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_proposal_version_audiences_proposal_versions] FOREIGN KEY ([proposal_version_id]) REFERENCES [dbo].[proposal_versions] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_audiences_proposal_versions]
    ON [dbo].[proposal_version_audiences]([proposal_version_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_audiences_audiences]
    ON [dbo].[proposal_version_audiences]([audience_id] ASC);

