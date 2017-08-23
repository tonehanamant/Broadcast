CREATE TABLE [dbo].[frozen_systems] (
    [media_month_id]        SMALLINT     NOT NULL,
    [id]                    INT          NOT NULL,
    [code]                  VARCHAR (15) NOT NULL,
    [name]                  VARCHAR (63) NOT NULL,
    [location]              VARCHAR (63) NOT NULL,
    [spot_yield_weight]     FLOAT (53)   NOT NULL,
    [traffic_order_format]  INT          NOT NULL,
    [flag]                  TINYINT      NULL,
    [custom_traffic_system] BIT          NOT NULL,
    CONSTRAINT [PK_frozen_systems] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [id] ASC) WITH (FILLFACTOR = 90)
);



