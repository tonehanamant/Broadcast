CREATE TABLE [dbo].[broadcast_scx_orders] (
    [id]                          INT             IDENTITY (1, 1) NOT NULL,
    [broadcast_scx_id]            INT             NOT NULL,
    [ncc_market_id]               INT             NULL,
    [strata_usebroadcastweeks_id] INT             NULL,
    [total_cost]                  DECIMAL (10, 2) NULL,
    [total_spots]                 INT             NULL,
    CONSTRAINT [PK_broadcast_scx_orders] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_broadcast_scx_orders_broadcast_scx] FOREIGN KEY ([broadcast_scx_id]) REFERENCES [dbo].[broadcast_scx] ([id]) ON DELETE CASCADE
);

