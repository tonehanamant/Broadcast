CREATE TABLE [dbo].[broadcast_scx_order_systemorder_weeks] (
    [id]                                 INT     IDENTITY (1, 1) NOT NULL,
    [broadcast_scx_order_systemorder_id] INT     NOT NULL,
    [week_number]                        TINYINT NULL,
    [start_date]                         DATE    NULL,
    CONSTRAINT [PK_broadcast_scx_order_systemorder_weeks] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_broadcast_scx_order_systemorder_weeks_broadcast_scx_order_systemorders] FOREIGN KEY ([broadcast_scx_order_systemorder_id]) REFERENCES [dbo].[broadcast_scx_order_systemorders] ([id]) ON DELETE CASCADE
);

