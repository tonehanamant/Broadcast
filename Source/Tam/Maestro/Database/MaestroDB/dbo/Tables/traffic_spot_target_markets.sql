CREATE TABLE [dbo].[traffic_spot_target_markets] (
    [traffic_spot_target_allocation_group_id] INT        NOT NULL,
    [market_id]                               INT        NOT NULL,
    [traffic_gross_cpm1]                      MONEY      NOT NULL,
    [traffic_gross_cpm2]                      MONEY      NULL,
    [subscribers]                             FLOAT (53) NOT NULL,
    [traffic_gross_cpm]                       MONEY      NOT NULL,
    CONSTRAINT [PK_traffic_spot_target_markets] PRIMARY KEY CLUSTERED ([traffic_spot_target_allocation_group_id] ASC, [market_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_spot_target_markets_traffic_spot_target_allocation_group] FOREIGN KEY ([traffic_spot_target_allocation_group_id]) REFERENCES [dbo].[traffic_spot_target_allocation_group] ([id])
);

