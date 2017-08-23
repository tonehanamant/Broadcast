CREATE TABLE [dbo].[traffic_detail_topographies] (
    [traffic_detail_week_id] INT        NOT NULL,
    [topography_id]          INT        NOT NULL,
    [daypart_id]             INT        NOT NULL,
    [spots]                  FLOAT (53) NOT NULL,
    [universe]               FLOAT (53) NOT NULL,
    [rate]                   MONEY      NOT NULL,
    [lookup_rate]            MONEY      NOT NULL,
    [ordered_spot_cost]      MONEY      NULL,
    [calculated_spot_cost]   MONEY      NULL,
    [fixed_spot_cost]        MONEY      NULL,
    [spot_cost1]             MONEY      NULL,
    [spot_cost2]             MONEY      NULL,
    CONSTRAINT [PK_traffic_detail_topographies] PRIMARY KEY CLUSTERED ([traffic_detail_week_id] ASC, [topography_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_detail_topographies_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_traffic_detail_topographies_topographies] FOREIGN KEY ([topography_id]) REFERENCES [dbo].[topographies] ([id])
);



