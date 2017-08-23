CREATE TABLE [dbo].[broadcast_scx_demos] (
    [id]               INT          IDENTITY (1, 1) NOT NULL,
    [broadcast_scx_id] INT          NOT NULL,
    [demo_rank]        TINYINT      NULL,
    [demo_group]       VARCHAR (63) NULL,
    [demo_population]  INT          NULL,
    [agefrom]          TINYINT      NULL,
    [ageto]            TINYINT      NULL,
    CONSTRAINT [PK_broadcast_scx_demos] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_broadcast_scx_demos_broadcast_scx] FOREIGN KEY ([broadcast_scx_id]) REFERENCES [dbo].[broadcast_scx] ([id]) ON DELETE CASCADE
);

