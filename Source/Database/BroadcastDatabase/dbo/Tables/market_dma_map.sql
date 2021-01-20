CREATE TABLE [dbo].[market_dma_map] (
    [market_code]      SMALLINT     NOT NULL,
    [dma_mapped_value] VARCHAR (63) NOT NULL,
    CONSTRAINT [PK_market_dma_map] PRIMARY KEY CLUSTERED ([market_code] ASC),
    CONSTRAINT [FK_market_dma_map_market_code] FOREIGN KEY ([market_code]) REFERENCES [dbo].[markets] ([market_code])
);

