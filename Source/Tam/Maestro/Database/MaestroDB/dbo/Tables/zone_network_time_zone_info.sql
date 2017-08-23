CREATE TABLE [dbo].[zone_network_time_zone_info] (
    [zone_id]                         INT      NOT NULL,
    [network_id]                      INT      NOT NULL,
    [nielsen_network_id]              INT      NOT NULL,
    [local_time_zone_id]              INT      NOT NULL,
    [feed_time_zone_id]               INT      NOT NULL,
    [daylight_savings_time_indicator] BIT      NOT NULL,
    [effective_date]                  DATETIME NOT NULL,
    CONSTRAINT [PK_zone_network_time_zone_info] PRIMARY KEY CLUSTERED ([zone_id] ASC, [network_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_zone_network_time_zone_info_2_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_zone_network_time_zone_info_2_time_zones_feed] FOREIGN KEY ([feed_time_zone_id]) REFERENCES [dbo].[time_zones] ([id]),
    CONSTRAINT [FK_zone_network_time_zone_info_2_time_zones_local] FOREIGN KEY ([local_time_zone_id]) REFERENCES [dbo].[time_zones] ([id]),
    CONSTRAINT [FK_zone_network_time_zone_info_2_zones] FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info', @level2type = N'COLUMN', @level2name = N'nielsen_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info', @level2type = N'COLUMN', @level2name = N'nielsen_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info', @level2type = N'COLUMN', @level2name = N'local_time_zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info', @level2type = N'COLUMN', @level2name = N'local_time_zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info', @level2type = N'COLUMN', @level2name = N'feed_time_zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info', @level2type = N'COLUMN', @level2name = N'feed_time_zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info', @level2type = N'COLUMN', @level2name = N'daylight_savings_time_indicator';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info', @level2type = N'COLUMN', @level2name = N'daylight_savings_time_indicator';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_network_time_zone_info', @level2type = N'COLUMN', @level2name = N'effective_date';

