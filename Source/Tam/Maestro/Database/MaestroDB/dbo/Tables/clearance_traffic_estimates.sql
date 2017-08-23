CREATE TABLE [dbo].[clearance_traffic_estimates] (
    [media_week_id]                INT        NOT NULL,
    [billing_system_id]            INT        NOT NULL,
    [billing_zone_id]              INT        NOT NULL,
    [network_id]                   INT        NOT NULL,
    [component_daypart_id]         INT        NOT NULL,
    [traffic_id]                   INT        NOT NULL,
    [component_daypart_percentage] FLOAT (53) NOT NULL,
    [daypart_ordered_spots]        FLOAT (53) NOT NULL,
    [scaled_ordered_spot_rate]     MONEY      NOT NULL,
    [trafficked_ordered_spot_rate] MONEY      NOT NULL,
    [allocated_availability]       FLOAT (53) NOT NULL,
    [billing_zone_subscribers]     INT        NOT NULL,
    CONSTRAINT [PK_clearance_traffic_estimates] PRIMARY KEY CLUSTERED ([media_week_id] ASC, [billing_system_id] ASC, [billing_zone_id] ASC, [network_id] ASC, [component_daypart_id] ASC, [traffic_id] ASC) WITH (FILLFACTOR = 90) ON [MediaWeekScheme] ([media_week_id]),
    CONSTRAINT [FK_clearance_traffic_estimates_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The adjusted ordered spot rate of the traffic order based on the potential subscriber change between the billing and traffic zone.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'scaled_ordered_spot_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'scaled_ordered_spot_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'trafficked_ordered_spot_rate';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The original ordered spot rate.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'trafficked_ordered_spot_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'trafficked_ordered_spot_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'allocated_availability';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The amount of availability that was predicted to be allocated to this traffic order record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'allocated_availability';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'allocated_availability';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'billing_zone_subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The billing zone records subscriber count.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'billing_zone_subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'billing_zone_subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'media_week_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The media week record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'media_week_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'media_week_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'billing_system_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The billing system record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'billing_system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'billing_system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'billing_zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The billing zone record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'billing_zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'billing_zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The network record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'component_daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The component daypart record (in components of 16, 8 M-F and 8 SA-SU)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'component_daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'component_daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The traffic record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'component_daypart_percentage';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The percentage used in distributing the ordered spots to the particular component daypart.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'component_daypart_percentage';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'component_daypart_percentage';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'daypart_ordered_spots';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The number of ordered spots specifically for the component daypart.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'daypart_ordered_spots';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'daypart_ordered_spots';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'clearance_traffic_estimates', @level2type = N'COLUMN', @level2name = N'scaled_ordered_spot_rate';

