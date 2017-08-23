CREATE TABLE [dbo].[broadcast_detail_markets] (
    [id]                           INT      IDENTITY (1, 1) NOT NULL,
    [market_code]                  SMALLINT NULL,
    [martket_rank]                 SMALLINT NULL,
    [broadcast_proposal_detail_id] INT      NOT NULL,
    [include]                      BIT      NOT NULL,
    CONSTRAINT [PK_broadcast_detail_markets] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_broadcast_detail_markets_broadcast_proposal_details] FOREIGN KEY ([broadcast_proposal_detail_id]) REFERENCES [dbo].[broadcast_proposal_details] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_detail_markets';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_detail_markets';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_detail_markets';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_detail_markets', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_detail_markets', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_detail_markets', @level2type = N'COLUMN', @level2name = N'market_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_detail_markets', @level2type = N'COLUMN', @level2name = N'market_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_detail_markets', @level2type = N'COLUMN', @level2name = N'martket_rank';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_detail_markets', @level2type = N'COLUMN', @level2name = N'martket_rank';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_detail_markets', @level2type = N'COLUMN', @level2name = N'broadcast_proposal_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_detail_markets', @level2type = N'COLUMN', @level2name = N'broadcast_proposal_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_detail_markets', @level2type = N'COLUMN', @level2name = N'include';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_detail_markets', @level2type = N'COLUMN', @level2name = N'include';

