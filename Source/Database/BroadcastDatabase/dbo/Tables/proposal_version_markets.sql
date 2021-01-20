CREATE TABLE [dbo].[proposal_version_markets] (
    [id]                  INT      IDENTITY (1, 1) NOT NULL,
    [proposal_version_id] INT      NOT NULL,
    [market_code]         SMALLINT NOT NULL,
    [is_blackout]         BIT      NOT NULL,
    CONSTRAINT [PK_proposal_version_markets] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_version_markets_markets] FOREIGN KEY ([market_code]) REFERENCES [dbo].[markets] ([market_code]),
    CONSTRAINT [FK_proposal_version_markets_proposal_versions] FOREIGN KEY ([proposal_version_id]) REFERENCES [dbo].[proposal_versions] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_markets_markets]
    ON [dbo].[proposal_version_markets]([market_code] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_markets_proposal_versions]
    ON [dbo].[proposal_version_markets]([proposal_version_id] ASC);

