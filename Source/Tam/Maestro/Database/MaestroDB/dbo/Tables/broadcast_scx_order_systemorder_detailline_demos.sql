CREATE TABLE [dbo].[broadcast_scx_order_systemorder_detailline_demos] (
    [id]                                            INT        IDENTITY (1, 1) NOT NULL,
    [broadcast_scx_order_systemorder_detailline_id] INT        NOT NULL,
    [demo_rank]                                     TINYINT    NULL,
    [ratings]                                       FLOAT (53) NULL,
    [impressions]                                   INT        NULL,
    CONSTRAINT [PK_broadcast_scx_order_systemorder_detailline_demos] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_broadcast_scx_order_systemorder_detailline_demos_broadcast_scx_order_systemorder_detaillines] FOREIGN KEY ([broadcast_scx_order_systemorder_detailline_id]) REFERENCES [dbo].[broadcast_scx_order_systemorder_detaillines] ([id]) ON DELETE CASCADE
);

