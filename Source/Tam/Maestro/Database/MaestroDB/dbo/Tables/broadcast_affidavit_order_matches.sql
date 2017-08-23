CREATE TABLE [dbo].[broadcast_affidavit_order_matches] (
    [id]                                            INT    IDENTITY (1, 1) NOT NULL,
    [broadcast_affidavit_id]                        BIGINT NOT NULL,
    [broadcast_scx_order_systemorder_detailline_id] INT    NULL,
    [market]                                        BIT    DEFAULT ((0)) NOT NULL,
    [air_time]                                      BIT    DEFAULT ((0)) NOT NULL,
    [station]                                       BIT    DEFAULT ((0)) NOT NULL,
    [program]                                       BIT    DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_broadcast_affidavit_order_matches] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_broadcast_affidavit_order_matches_broadcast_scx_order_systemorder_detaillines] FOREIGN KEY ([broadcast_scx_order_systemorder_detailline_id]) REFERENCES [dbo].[broadcast_scx_order_systemorder_detaillines] ([id]) ON DELETE CASCADE
);

