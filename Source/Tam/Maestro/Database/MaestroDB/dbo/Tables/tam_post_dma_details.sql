CREATE TABLE [dbo].[tam_post_dma_details] (
    [id]                   INT        IDENTITY (1, 1) NOT NULL,
    [enabled]              BIT        NOT NULL,
    [tam_post_proposal_id] INT        NOT NULL,
    [dma_id]               INT        NOT NULL,
    [network_id]           INT        NOT NULL,
    [subscribers]          BIGINT     NOT NULL,
    [units]                FLOAT (53) NOT NULL,
    [total_spots]          INT        NULL,
    CONSTRAINT [PK_tam_post_dma_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_tam_post_dma_details_dmas] FOREIGN KEY ([dma_id]) REFERENCES [dbo].[dmas] ([id]),
    CONSTRAINT [FK_tam_post_dma_details_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_tam_post_dma_details_tam_post_proposals] FOREIGN KEY ([tam_post_proposal_id]) REFERENCES [dbo].[tam_post_proposals] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_tam_post_dma_details]
    ON [dbo].[tam_post_dma_details]([tam_post_proposal_id] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains the DMA details for a posting plan in a post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'TAM Post DMA Details', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Gives you the subscribers and direct response units for a DMA/network.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Whether or not the data counts towards the post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'enabled';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'When false, the data has been excluded from the post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'enabled';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the posting plan and post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'tam_post_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'tam_post_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the DMA.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'dma_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'dma_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the network.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Total subscribers for the DMA/network.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Total direct response units for the DMA/network.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'units';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'units';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'total_spots';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_details', @level2type = N'COLUMN', @level2name = N'total_spots';

