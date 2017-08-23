CREATE TABLE [dbo].[broadcast_traffic_detail_audiences] (
    [broadcast_proposal_detail_id] INT      NOT NULL,
    [audience_id]                  INT      NOT NULL,
    [market_code]                  SMALLINT NOT NULL,
    [subscribers]                  INT      NOT NULL,
    CONSTRAINT [PK_broadcast_traffic_detail_audiences] PRIMARY KEY CLUSTERED ([broadcast_proposal_detail_id] ASC, [audience_id] ASC, [market_code] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_broadcast_traffic_detail_audiences_proposal_detail] FOREIGN KEY ([broadcast_proposal_detail_id]) REFERENCES [dbo].[broadcast_proposal_details] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'broadcast_proposal_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'broadcast_proposal_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'market_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'market_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'subscribers';

