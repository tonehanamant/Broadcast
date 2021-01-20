CREATE TABLE [dbo].[proposal_version_spot_length] (
    [id]                  INT IDENTITY (1, 1) NOT NULL,
    [proposal_version_id] INT NOT NULL,
    [spot_length_id]      INT NOT NULL,
    CONSTRAINT [PK_proposal_version_spot_length] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_version_spot_length_proposal_versions] FOREIGN KEY ([proposal_version_id]) REFERENCES [dbo].[proposal_versions] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_proposal_version_spot_length_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_spot_length_proposal_versions]
    ON [dbo].[proposal_version_spot_length]([proposal_version_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_spot_length_spot_lengths]
    ON [dbo].[proposal_version_spot_length]([spot_length_id] ASC);

