CREATE TABLE [dbo].[network_map_histories] (
    [network_map_id] INT          NOT NULL,
    [start_date]     DATETIME     NOT NULL,
    [network_id]     INT          NOT NULL,
    [map_set]        VARCHAR (15) NOT NULL,
    [map_value]      VARCHAR (63) NOT NULL,
    [active]         BIT          NOT NULL,
    [flag]           TINYINT      CONSTRAINT [DF_network_map_histories_flag] DEFAULT ((1)) NULL,
    [end_date]       DATETIME     NOT NULL,
    CONSTRAINT [PK_network_map_histories] PRIMARY KEY CLUSTERED ([network_map_id] ASC, [start_date] ASC),
    CONSTRAINT [FK_network_map_histories_network_maps] FOREIGN KEY ([network_map_id]) REFERENCES [dbo].[network_maps] ([id]),
    CONSTRAINT [FK_network_map_histories_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains multiple map sets which pair invalid network abbreviations with an index into our networks table.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Network Map History', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Use this table to lookup what a map value was for a specific period of time.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the network record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'network_map_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'network_map_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Starting date this data was accurate.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Logical grouping of network map entries.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'map_set';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'map_set';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The invalid value being mapped.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'map_value';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'map_value';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Ending date this data was accurate.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_map_histories', @level2type = N'COLUMN', @level2name = N'end_date';

