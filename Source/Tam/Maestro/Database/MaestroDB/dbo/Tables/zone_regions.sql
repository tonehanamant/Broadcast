CREATE TABLE [dbo].[zone_regions] (
    [id]             INT      IDENTITY (1, 1) NOT NULL,
    [zone_id]        INT      NOT NULL,
    [region_id]      INT      NOT NULL,
    [effective_date] DATETIME NULL,
    [is_active]      BIT      DEFAULT ((0)) NULL,
    CONSTRAINT [PK_zone_regions] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_zone_regions_regions_id] FOREIGN KEY ([region_id]) REFERENCES [dbo].[regions] ([id]),
    CONSTRAINT [FK_zone_regions_zones_id] FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);

