CREATE TABLE [dbo].[tam_post_dma_detail_audiences] (
    [tam_post_dma_detail_id] INT        NOT NULL,
    [audience_id]            INT        NOT NULL,
    [delivery]               FLOAT (53) NOT NULL,
    [eq_delivery]            FLOAT (53) NOT NULL,
    [dr_delivery]            FLOAT (53) NOT NULL,
    [dr_eq_delivery]         FLOAT (53) NOT NULL,
    CONSTRAINT [PK_tam_post_dma_detail_audiences] PRIMARY KEY CLUSTERED ([tam_post_dma_detail_id] ASC, [audience_id] ASC),
    CONSTRAINT [FK_tam_post_dma_detail_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_tam_post_dma_detail_audiences_tam_post_dma_details] FOREIGN KEY ([tam_post_dma_detail_id]) REFERENCES [dbo].[tam_post_dma_details] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains the delivery information for all demographics for a specific TAM Post DMA Detail.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'TAM Post DMA Detail Audiences', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Contains every variation of delivery (brand/dr) and (equivalized or not).', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the TAM Post Dma Detail.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences', @level2type = N'COLUMN', @level2name = N'tam_post_dma_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences', @level2type = N'COLUMN', @level2name = N'tam_post_dma_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized delivery for this TAM Post Dma Detail.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences', @level2type = N'COLUMN', @level2name = N'delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences', @level2type = N'COLUMN', @level2name = N'delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized delivery for this TAM Post Dma Detail.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences', @level2type = N'COLUMN', @level2name = N'eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences', @level2type = N'COLUMN', @level2name = N'eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized direct response delivery for this TAM Post Dma Detail.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences', @level2type = N'COLUMN', @level2name = N'dr_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences', @level2type = N'COLUMN', @level2name = N'dr_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized direct response delivery for this TAM Post Dma Detail.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_dma_detail_audiences', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery';

