CREATE TABLE [dbo].[traffic_alerts] (
    [id]                    INT           IDENTITY (1, 1) NOT NULL,
    [alert_comment]         VARCHAR (255) NULL,
    [traffic_id]            INT           NOT NULL,
    [traffic_alert_type_id] INT           NOT NULL,
    [copy_comment]          VARCHAR (255) NULL,
    [effective_date]        DATETIME      NULL,
    [last_modified_date]    DATETIME      NULL,
    [last_modified_user]    INT           NULL,
    CONSTRAINT [PK_traffic_alerts_1] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_alerts_employees] FOREIGN KEY ([last_modified_user]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_traffic_alerts_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id]),
    CONSTRAINT [FK_traffic_alerts_traffic_alert_types] FOREIGN KEY ([traffic_alert_type_id]) REFERENCES [dbo].[traffic_alert_types] ([id])
);




GO
CREATE NONCLUSTERED INDEX [IX_traffic_alerts_traffic_id]
    ON [dbo].[traffic_alerts]([traffic_id] ASC) WITH (FILLFACTOR = 90);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alerts';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alerts';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alerts';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alerts', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alerts', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alerts', @level2type = N'COLUMN', @level2name = N'alert_comment';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alerts', @level2type = N'COLUMN', @level2name = N'alert_comment';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alerts', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alerts', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alerts', @level2type = N'COLUMN', @level2name = N'traffic_alert_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alerts', @level2type = N'COLUMN', @level2name = N'traffic_alert_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alerts', @level2type = N'COLUMN', @level2name = N'copy_comment';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alerts', @level2type = N'COLUMN', @level2name = N'copy_comment';

