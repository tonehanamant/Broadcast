CREATE TABLE [dbo].[frozen_primary_subscribers] (
    [media_month_id]      SMALLINT NOT NULL,
    [network_id]          INT      NOT NULL,
    [zone_id]             INT      NOT NULL,
    [subscribers]         INT      NOT NULL,
    [managed_business_id] INT      NOT NULL,
    [dma_id]              INT      NOT NULL,
    CONSTRAINT [PK_frozen_primary_subscribers] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [network_id] ASC, [managed_business_id] ASC, [dma_id] ASC, [zone_id] ASC) WITH (FILLFACTOR = 90)
);

