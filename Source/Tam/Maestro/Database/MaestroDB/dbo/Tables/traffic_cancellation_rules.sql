CREATE TABLE [dbo].[traffic_cancellation_rules] (
    [id]               INT IDENTITY (1, 1) NOT NULL,
    [rule_id]          INT NOT NULL,
    [traffic_alert_id] INT NOT NULL,
    CONSTRAINT [PK_traffic_cancellation_rules] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_traffic_cancellation_rules_rules] FOREIGN KEY ([rule_id]) REFERENCES [dbo].[rules] ([id]),
    CONSTRAINT [FK_traffic_cancellation_rules_traffic_alerts] FOREIGN KEY ([traffic_alert_id]) REFERENCES [dbo].[traffic_alerts] ([id])
);

