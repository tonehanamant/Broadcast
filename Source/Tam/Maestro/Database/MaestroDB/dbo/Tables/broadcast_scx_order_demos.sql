CREATE TABLE [dbo].[broadcast_scx_order_demos] (
    [id]                     INT     IDENTITY (1, 1) NOT NULL,
    [broadcast_scx_order_id] INT     NOT NULL,
    [demo_rank]              TINYINT NULL,
    [demo_population]        INT     NULL,
    CONSTRAINT [PK_broadcast_scx_order_demos] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_broadcast_scx_order_demos_broadcast_scx_orders] FOREIGN KEY ([broadcast_scx_order_id]) REFERENCES [dbo].[broadcast_scx_orders] ([id]) ON DELETE CASCADE
);

