CREATE TABLE [dbo].[frozen_zones] (
    [media_month_id] SMALLINT     NOT NULL,
    [id]             INT          NOT NULL,
    [code]           VARCHAR (15) NOT NULL,
    [name]           VARCHAR (63) NOT NULL,
    [type]           VARCHAR (63) NOT NULL,
    [primary]        BIT          NOT NULL,
    [traffic]        BIT          NOT NULL,
    [dma]            BIT          NOT NULL,
    [flag]           TINYINT      NULL,
    CONSTRAINT [PK_frozen_zones] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [id] ASC)
);

