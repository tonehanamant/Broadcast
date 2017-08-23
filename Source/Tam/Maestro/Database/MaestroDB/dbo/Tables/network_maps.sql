CREATE TABLE [dbo].[network_maps] (
    [id]             INT          IDENTITY (1, 1) NOT NULL,
    [network_id]     INT          NOT NULL,
    [map_set]        VARCHAR (15) NOT NULL,
    [map_value]      VARCHAR (63) NOT NULL,
    [active]         BIT          NOT NULL,
    [flag]           TINYINT      CONSTRAINT [DF__network_ma__flag__5D16C24D] DEFAULT ((1)) NULL,
    [effective_date] DATETIME     NOT NULL,
    CONSTRAINT [PK_network_maps] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_network_maps_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_newtork_maps]
    ON [dbo].[network_maps]([map_set] ASC)
    INCLUDE([network_id], [map_value]);


GO
CREATE NONCLUSTERED INDEX [IX_newtork_maps_2]
    ON [dbo].[network_maps]([network_id] ASC, [map_set] ASC)
    INCLUDE([map_value]);


GO
CREATE STATISTICS [_dta_stat_1649440950_4_3]
    ON [dbo].[network_maps]([map_value], [map_set]);


GO
CREATE STATISTICS [_dta_stat_1649440950_2_3]
    ON [dbo].[network_maps]([network_id], [map_set]);


GO
CREATE STATISTICS [_dta_stat_1649440950_5_3]
    ON [dbo].[network_maps]([active], [map_set]);


GO
CREATE STATISTICS [_dta_stat_1649440950_3_5_2]
    ON [dbo].[network_maps]([map_set], [active], [network_id]);


GO
CREATE STATISTICS [_dta_stat_1649440950_4_5_3_2]
    ON [dbo].[network_maps]([map_value], [active], [map_set], [network_id]);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Network Maps', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps', @level2type = N'COLUMN', @level2name = N'map_set';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps', @level2type = N'COLUMN', @level2name = N'map_set';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps', @level2type = N'COLUMN', @level2name = N'map_value';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps', @level2type = N'COLUMN', @level2name = N'map_value';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_maps', @level2type = N'COLUMN', @level2name = N'effective_date';

