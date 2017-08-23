CREATE TABLE [dbo].[DMA_UEs] (
    [DMA Code]                      INT            NULL,
    [DMA Rank]                      INT            NULL,
    [DMA Name]                      NVARCHAR (255) NULL,
    [TV HHs]                        FLOAT (53)     NULL,
    [Cable HHs]                     FLOAT (53)     NULL,
    [Cable with Pay HHs]            FLOAT (53)     NULL,
    [Cable Non-ADS HHs]             FLOAT (53)     NULL,
    [Digital Cable HHs]             FLOAT (53)     NULL,
    [Cable and/or ADS HHs]          FLOAT (53)     NULL,
    [Cable and/or ADS with Pay HHs] FLOAT (53)     NULL,
    [Broadcast Only HHs]            FLOAT (53)     NULL,
    [ADS HHs]                       FLOAT (53)     NULL,
    [DBS HHs]                       FLOAT (53)     NULL,
    [ADS Non-Cable HHs]             FLOAT (53)     NULL,
    [ADS and Cable (both) HHs]      FLOAT (53)     NULL,
    [2+ Operable TV Sets HHs]       FLOAT (53)     NULL,
    [VCR HHs]                       FLOAT (53)     NULL,
    [DVD HHs]                       FLOAT (53)     NULL
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'DMA Code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'DMA Code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'DMA Rank';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'DMA Rank';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'DMA Name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'DMA Name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'TV HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'TV HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'Cable HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'Cable HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'Cable with Pay HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'Cable with Pay HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'Cable Non-ADS HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'Cable Non-ADS HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'Digital Cable HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'Digital Cable HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'Cable and/or ADS HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'Cable and/or ADS HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'Cable and/or ADS with Pay HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'Cable and/or ADS with Pay HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'Broadcast Only HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'Broadcast Only HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'ADS HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'ADS HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'DBS HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'DBS HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'ADS Non-Cable HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'ADS Non-Cable HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'ADS and Cable (both) HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'ADS and Cable (both) HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'2+ Operable TV Sets HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'2+ Operable TV Sets HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'VCR HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'VCR HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'DVD HHs';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DMA_UEs', @level2type = N'COLUMN', @level2name = N'DVD HHs';

