CREATE TABLE [dbo].[broadcast_scx_order_systemorder_demos] (
    [id]                                 INT     IDENTITY (1, 1) NOT NULL,
    [broadcast_scx_order_systemorder_id] INT     NOT NULL,
    [demo_rank]                          TINYINT NULL,
    [demo_population]                    INT     NULL,
    CONSTRAINT [PK_broadcast_scx_order_systemorder_demos] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_broadcast_scx_order_systemorder_demos_broadcast_scx_order_systemorders] FOREIGN KEY ([broadcast_scx_order_systemorder_id]) REFERENCES [dbo].[broadcast_scx_order_systemorders] ([id]) ON DELETE CASCADE
);

