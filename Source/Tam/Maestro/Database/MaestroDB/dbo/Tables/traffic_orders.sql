CREATE TABLE [dbo].[traffic_orders] (
    [id]                               INT        IDENTITY (1, 1) NOT NULL,
    [system_id]                        INT        NOT NULL,
    [zone_id]                          INT        NOT NULL,
    [traffic_detail_id]                INT        NOT NULL,
    [daypart_id]                       INT        NOT NULL,
    [ordered_spots]                    INT        NOT NULL,
    [ordered_spot_rate]                MONEY      NOT NULL,
    [start_date]                       DATETIME   NOT NULL,
    [end_date]                         DATETIME   NOT NULL,
    [release_id]                       INT        NULL,
    [subscribers]                      INT        NULL,
    [display_network_id]               INT        NOT NULL,
    [on_financial_reports]             BIT        CONSTRAINT [DF_traffic_orders_on_financial_reports] DEFAULT ((1)) NOT NULL,
    [active]                           BIT        CONSTRAINT [DF_traffic_orders_active] DEFAULT ((1)) NOT NULL,
    [topography_id]                    INT        NULL,
    [proposal1_rating]                 FLOAT (53) NULL,
    [proposal2_rating]                 FLOAT (53) NULL,
    [proposal1_guaranteed_audience_id] INT        NULL,
    [proposal2_guaranteed_audience_id] INT        NULL,
    [proposal1_3net_cpm]               MONEY      NULL,
    [proposal2_3net_cpm]               MONEY      NULL,
    [traffic1_rating]                  FLOAT (53) NULL,
    [traffic2_rating]                  FLOAT (53) NULL,
    [proposal1_id]                     INT        NULL,
    [proposal2_id]                     INT        NULL,
    [discount_factor]                  FLOAT (53) NULL,
    [original_rate]                    MONEY      NULL,
    [calculated_rate]                  MONEY      NULL,
    [rate1]                            MONEY      NULL,
    [rate2]                            MONEY      NULL,
    [traffic_spot_target_id]           INT        NULL,
    [media_month_id]                   SMALLINT   NOT NULL,
    [media_week_id]                    INT        NOT NULL,
    [traffic_id]                       INT        NOT NULL,
    CONSTRAINT [PK_traffic_orders] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [id] ASC) WITH (FILLFACTOR = 90) ON [MediaMonthSmallintScheme] ([media_month_id]),
    CONSTRAINT [FK_traffic_orders_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_traffic_orders_networks] FOREIGN KEY ([display_network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_traffic_orders_releases] FOREIGN KEY ([release_id]) REFERENCES [dbo].[releases] ([id]),
    CONSTRAINT [FK_traffic_orders_traffic_details] FOREIGN KEY ([traffic_detail_id]) REFERENCES [dbo].[traffic_details] ([id]),
    CONSTRAINT [FK_traffic_orders_traffic_orders] FOREIGN KEY ([system_id]) REFERENCES [dbo].[systems] ([id]),
    CONSTRAINT [FK_traffic_orders_zones] FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);


GO
ALTER TABLE [dbo].[traffic_orders] NOCHECK CONSTRAINT [FK_traffic_orders_dayparts];




GO
CREATE UNIQUE NONCLUSTERED INDEX [Ind_traffic_orders_unique_key]
    ON [dbo].[traffic_orders]([traffic_detail_id] ASC, [system_id] ASC, [zone_id] ASC, [display_network_id] ASC, [start_date] ASC, [end_date] ASC, [daypart_id] ASC, [media_month_id] ASC) WITH (FILLFACTOR = 90)
    ON [MediaMonthSmallintScheme] ([media_month_id]);




GO



GO



GO



GO



GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'System record trafficked to', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Zone record trafficked to.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'traffic_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The source traffic detail record which is by network at the national level.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'traffic_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'traffic_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The daypart record trafficked to.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'ordered_spots';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The number of ordered spots or units.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'ordered_spots';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'ordered_spots';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'ordered_spot_rate';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The ordered rate per spot, this is the rate we are willing to pay per spot.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'ordered_spot_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'ordered_spot_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The start date of the ordered spots, always falls in a media week.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The end date of the ordered spots, always falls in a media week.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'release_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The corresponding release record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'release_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'release_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The subscribers in which the spot rate calculation was based on, this is the number of subscribers for the zone/network based on the start date of the traffic order.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'display_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'This is the network displayed on the traffic order. It''s used to map networks like FXSP to the actual regional sports network of the zone.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'display_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'display_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'on_financial_reports';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Combined with active below, both booleans, if both are true then the record should be reported on, if both false it should be excluded.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'on_financial_reports';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'on_financial_reports';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_orders', @level2type = N'COLUMN', @level2name = N'active';


GO
CREATE NONCLUSTERED INDEX [IX_traffic_orders_traffic_spot_target_id]
    ON [dbo].[traffic_orders]([traffic_id] ASC, [traffic_spot_target_id] ASC)
    INCLUDE([media_month_id], [id], [traffic_detail_id], [release_id], [system_id], [zone_id], [topography_id], [on_financial_reports]) WITH (FILLFACTOR = 90)
    ON [MediaMonthSmallintScheme] ([media_month_id]);


GO
CREATE NONCLUSTERED INDEX [IX_traffic_orders_ims]
    ON [dbo].[traffic_orders]([media_month_id] ASC, [on_financial_reports] ASC, [active] ASC, [ordered_spots] ASC, [ordered_spot_rate] ASC)
    INCLUDE([media_week_id], [traffic_id], [traffic_detail_id], [traffic_spot_target_id], [subscribers])
    ON [MediaMonthSmallintScheme] ([media_month_id]);

