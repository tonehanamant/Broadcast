CREATE TABLE [dbo].[frozen_topography_dmas] (
    [media_month_id] SMALLINT NOT NULL,
    [topography_id]  INT      NOT NULL,
    [dma_id]         INT      NOT NULL,
    [include]        BIT      NOT NULL,
    CONSTRAINT [PK_frozen_topography_dmas] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [topography_id] ASC, [dma_id] ASC)
);

