CREATE TABLE [dbo].[frozen_zone_zones] (
    [media_month_id]    SMALLINT     NOT NULL,
    [primary_zone_id]   INT          NOT NULL,
    [secondary_zone_id] INT          NOT NULL,
    [type]              VARCHAR (15) NOT NULL,
    CONSTRAINT [PK_frozen_zone_zones] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [primary_zone_id] ASC, [secondary_zone_id] ASC, [type] ASC)
);

