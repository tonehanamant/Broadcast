CREATE TABLE [dbo].[frozen_dmas] (
    [media_month_id] SMALLINT     NOT NULL,
    [dma_id]         INT          NOT NULL,
    [code]           VARCHAR (15) NOT NULL,
    [name]           VARCHAR (63) NOT NULL,
    [rank]           INT          NOT NULL,
    [tv_hh]          INT          NOT NULL,
    [cable_hh]       INT          NOT NULL,
    [flag]           TINYINT      NULL,
    CONSTRAINT [PK_frozen_dmas] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [dma_id] ASC) WITH (FILLFACTOR = 90)
);

