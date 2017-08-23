CREATE TABLE [dbo].[frozen_traffic_network_map] (
    [media_month_id]     SMALLINT NOT NULL,
    [traffic_network_id] INT      NOT NULL,
    [zone_network_id]    INT      NOT NULL,
    CONSTRAINT [PK_frozen_traffic_network_map] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [traffic_network_id] ASC, [zone_network_id] ASC) WITH (FILLFACTOR = 90)
);

