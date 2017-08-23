CREATE TABLE [dbo].[traffic_footprint_rule_states] (
    [traffic_footprint_rule_id] INT NOT NULL,
    [state_id]                  INT NOT NULL,
    CONSTRAINT [PK_traffic_footprint_rule_states] PRIMARY KEY CLUSTERED ([traffic_footprint_rule_id] ASC, [state_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_footprint_rule_states_states] FOREIGN KEY ([state_id]) REFERENCES [dbo].[states] ([id]),
    CONSTRAINT [FK_traffic_footprint_rule_states_traffic_footprint_rules] FOREIGN KEY ([traffic_footprint_rule_id]) REFERENCES [dbo].[traffic_footprint_rules] ([id])
);

