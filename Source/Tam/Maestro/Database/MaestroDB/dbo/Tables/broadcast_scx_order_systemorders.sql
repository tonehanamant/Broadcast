CREATE TABLE [dbo].[broadcast_scx_order_systemorders] (
    [id]                     INT             IDENTITY (1, 1) NOT NULL,
    [broadcast_scx_order_id] INT             NOT NULL,
    [stata_usezonepop]       BIT             NULL,
    [total_cost]             DECIMAL (10, 2) NULL,
    [total_spots]            INT             NULL,
    [weeks_count]            TINYINT         NULL,
    CONSTRAINT [PK_broadcast_scx_order_systemorders] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_broadcast_scx_order_systemorders_broadcast_scx_orders] FOREIGN KEY ([broadcast_scx_order_id]) REFERENCES [dbo].[broadcast_scx_orders] ([id]) ON DELETE CASCADE
);

