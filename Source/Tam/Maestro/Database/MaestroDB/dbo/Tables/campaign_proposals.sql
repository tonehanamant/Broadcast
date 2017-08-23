CREATE TABLE [dbo].[campaign_proposals] (
    [proposal_id] INT NOT NULL,
    [campaign_id] INT NOT NULL,
    CONSTRAINT [PK_campaign_proposals] PRIMARY KEY CLUSTERED ([proposal_id] ASC, [campaign_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_campaign_proposals_campaigns] FOREIGN KEY ([campaign_id]) REFERENCES [dbo].[campaigns] ([id]),
    CONSTRAINT [FK_campaign_proposals_proposals] FOREIGN KEY ([proposal_id]) REFERENCES [dbo].[proposals] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'campaign_proposals';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'campaign_proposals';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'campaign_proposals';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'campaign_proposals', @level2type = N'COLUMN', @level2name = N'proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'campaign_proposals', @level2type = N'COLUMN', @level2name = N'proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'campaign_proposals', @level2type = N'COLUMN', @level2name = N'campaign_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'campaign_proposals', @level2type = N'COLUMN', @level2name = N'campaign_id';

