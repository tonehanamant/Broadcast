CREATE TABLE [dbo].[traffic_master_alert_traffic_alerts] (
    [id]                      INT IDENTITY (1, 1) NOT NULL,
    [traffic_master_alert_id] INT NOT NULL,
    [traffic_alert_id]        INT NOT NULL,
    [rank]                    INT NULL,
    CONSTRAINT [PK_traffic_master_alert_groups_1] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_master_alert_traffic_alerts_traffic_alerts] FOREIGN KEY ([traffic_alert_id]) REFERENCES [dbo].[traffic_alerts] ([id]),
    CONSTRAINT [FK_traffic_master_alert_traffic_alerts_traffic_master_alerts] FOREIGN KEY ([traffic_master_alert_id]) REFERENCES [dbo].[traffic_master_alerts] ([id]),
    CONSTRAINT [IX_traffic_master_alert_traffic_alerts] UNIQUE NONCLUSTERED ([traffic_master_alert_id] ASC, [traffic_alert_id] ASC) WITH (FILLFACTOR = 90)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_master_alert_traffic_alerts';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_master_alert_traffic_alerts';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_master_alert_traffic_alerts';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_master_alert_traffic_alerts', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_master_alert_traffic_alerts', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_master_alert_traffic_alerts', @level2type = N'COLUMN', @level2name = N'traffic_master_alert_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_master_alert_traffic_alerts', @level2type = N'COLUMN', @level2name = N'traffic_master_alert_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_master_alert_traffic_alerts', @level2type = N'COLUMN', @level2name = N'traffic_alert_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_master_alert_traffic_alerts', @level2type = N'COLUMN', @level2name = N'traffic_alert_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_master_alert_traffic_alerts', @level2type = N'COLUMN', @level2name = N'rank';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_master_alert_traffic_alerts', @level2type = N'COLUMN', @level2name = N'rank';

