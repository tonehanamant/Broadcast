CREATE TABLE [dbo].[frozen_topography_zones] (
    [media_month_id] SMALLINT NOT NULL,
    [topography_id]  INT      NOT NULL,
    [zone_id]        INT      NOT NULL,
    [include]        BIT      NOT NULL,
    CONSTRAINT [PK_frozen_topography_zone] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [topography_id] ASC, [zone_id] ASC)
);

