CREATE TABLE [dbo].[frozen_zone_dmas] (
    [media_month_id] SMALLINT   NOT NULL,
    [zone_id]        INT        NOT NULL,
    [dma_id]         INT        NOT NULL,
    [weight]         FLOAT (53) NOT NULL,
    CONSTRAINT [PK_frozen_zone_dmas] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [zone_id] ASC, [dma_id] ASC)
);

