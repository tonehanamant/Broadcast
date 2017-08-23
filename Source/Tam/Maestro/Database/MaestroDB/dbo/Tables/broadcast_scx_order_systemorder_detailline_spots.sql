CREATE TABLE [dbo].[broadcast_scx_order_systemorder_detailline_spots] (
    [id]                                            INT     IDENTITY (1, 1) NOT NULL,
    [broadcast_scx_order_systemorder_detailline_id] INT     NOT NULL,
    [week_number]                                   TINYINT NULL,
    [quantity]                                      INT     NULL,
    CONSTRAINT [PK_broadcast_scx_order_systemorder_detailline_spots] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_broadcast_scx_order_systemorder_detailline_spots_broadcast_scx_order_systemorder_detaillines] FOREIGN KEY ([broadcast_scx_order_systemorder_detailline_id]) REFERENCES [dbo].[broadcast_scx_order_systemorder_detaillines] ([id]) ON DELETE CASCADE
);

