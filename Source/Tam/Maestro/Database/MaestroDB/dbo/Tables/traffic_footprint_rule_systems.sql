CREATE TABLE [dbo].[traffic_footprint_rule_systems] (
    [traffic_footprint_rule_id] INT NOT NULL,
    [system_id]                 INT NOT NULL,
    CONSTRAINT [PK_traffic_footprint_rule_systems] PRIMARY KEY CLUSTERED ([traffic_footprint_rule_id] ASC, [system_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_footprint_rule_systems_systems] FOREIGN KEY ([system_id]) REFERENCES [dbo].[systems] ([id]),
    CONSTRAINT [FK_traffic_footprint_rule_systems_traffic_footprint_rules] FOREIGN KEY ([traffic_footprint_rule_id]) REFERENCES [dbo].[traffic_footprint_rules] ([id])
);

