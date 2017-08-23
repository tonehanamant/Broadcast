CREATE TABLE [dbo].[frozen_zone_networks] (
    [media_month_id] SMALLINT     NOT NULL,
    [zone_id]        INT          NOT NULL,
    [network_id]     INT          NOT NULL,
    [source]         VARCHAR (15) NOT NULL,
    [trafficable]    BIT          NOT NULL,
    [primary]        BIT          NOT NULL,
    [subscribers]    INT          NOT NULL,
    CONSTRAINT [PK_frozen_zone_networks] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [zone_id] ASC, [network_id] ASC)
);

