CREATE TABLE [dbo].[traffic_footprint_rule_mvpds] (
    [traffic_footprint_rule_id] INT NOT NULL,
    [mvpd_id]                   INT NOT NULL,
    CONSTRAINT [PK_traffic_footprint_rule_mvpds] PRIMARY KEY CLUSTERED ([traffic_footprint_rule_id] ASC, [mvpd_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_footprint_rule_mvpds_businesses] FOREIGN KEY ([mvpd_id]) REFERENCES [dbo].[businesses] ([id]),
    CONSTRAINT [FK_traffic_footprint_rule_mvpds_traffic_footprint_rules] FOREIGN KEY ([traffic_footprint_rule_id]) REFERENCES [dbo].[traffic_footprint_rules] ([id])
);

