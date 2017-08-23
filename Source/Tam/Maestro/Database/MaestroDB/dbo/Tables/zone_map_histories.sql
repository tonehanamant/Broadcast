CREATE TABLE [dbo].[zone_map_histories] (
    [zone_map_id] INT          NOT NULL,
    [start_date]  DATETIME     NOT NULL,
    [zone_id]     INT          NOT NULL,
    [map_set]     VARCHAR (15) NOT NULL,
    [map_value]   VARCHAR (63) NOT NULL,
    [weight]      FLOAT (53)   NOT NULL,
    [flag]        TINYINT      CONSTRAINT [DF_zone_map_histories_flag] DEFAULT ((1)) NULL,
    [active]      BIT          NOT NULL,
    [end_date]    DATETIME     NOT NULL,
    CONSTRAINT [PK_zone_map_histories] PRIMARY KEY CLUSTERED ([zone_map_id] ASC, [start_date] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_zone_map_histories_zone_maps] FOREIGN KEY ([zone_map_id]) REFERENCES [dbo].[zone_maps] ([id]),
    CONSTRAINT [FK_zone_map_histories_zones] FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains multiple map sets which pair invalid zone names with an index into our zones table.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Zone Maps History', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Use this table to lookup what a map value was for a specific period of time.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the zone record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'zone_map_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'zone_map_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Starting date this data was accurate.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Logical grouping of zone map entries.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'map_set';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'map_set';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The invalid value being mapped.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'map_value';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'map_value';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'weight';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'weight';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Ending date this data was accurate.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_map_histories', @level2type = N'COLUMN', @level2name = N'end_date';

