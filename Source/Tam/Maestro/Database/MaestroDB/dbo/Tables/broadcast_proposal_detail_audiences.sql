CREATE TABLE [dbo].[broadcast_proposal_detail_audiences] (
    [broadcast_proposal_detail_id] INT      NOT NULL,
    [audience_id]                  INT      NOT NULL,
    [ordinal]                      SMALLINT NOT NULL,
    CONSTRAINT [PK_broadcast_proposal_audiences] PRIMARY KEY CLUSTERED ([broadcast_proposal_detail_id] ASC, [audience_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_broadcast_proposal_detail_audiences_broadcast_proposal_detail] FOREIGN KEY ([broadcast_proposal_detail_id]) REFERENCES [dbo].[broadcast_proposal_details] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'broadcast_proposal_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'broadcast_proposal_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'ordinal';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'ordinal';

