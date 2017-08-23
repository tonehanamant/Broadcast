CREATE TABLE [dbo].[zone_regions_histories] (
    [zone_region_id] INT      NOT NULL,
    [zone_id]        INT      NOT NULL,
    [region_id]      INT      NOT NULL,
    [start_date]     DATETIME NOT NULL,
    [end_date]       DATETIME NULL,
    [is_active]      BIT      CONSTRAINT [DF_zone_regions_histories_IsActive] DEFAULT ((0)) NULL,
    CONSTRAINT [pk_zone_region_histories] PRIMARY KEY CLUSTERED ([zone_region_id] ASC, [zone_id] ASC, [region_id] ASC, [start_date] ASC),
    CONSTRAINT [FK_zone_regions_histories_regions_id] FOREIGN KEY ([region_id]) REFERENCES [dbo].[regions] ([id]),
    CONSTRAINT [FK_zone_regions_histories_zone_regions_id] FOREIGN KEY ([zone_region_id]) REFERENCES [dbo].[zone_regions] ([id]),
    CONSTRAINT [FK_zone_regions_histories_zones_id] FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);

