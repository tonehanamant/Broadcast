CREATE TABLE [dbo].[broadcast_traffic_detail_statelog] (
    [id]                          INT      IDENTITY (1, 1) NOT NULL,
    [broadcast_traffic_detail_id] INT      NOT NULL,
    [effective_date]              DATETIME NULL,
    [accepted]                    BIT      NOT NULL,
    [employee_id]                 INT      NULL,
    CONSTRAINT [PK_broadcast_traffic_detail_statelog] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_broadcast_traffic_detail_statelog_broadcast_traffic_details] FOREIGN KEY ([broadcast_traffic_detail_id]) REFERENCES [dbo].[broadcast_traffic_details] ([id]),
    CONSTRAINT [FK_broadcast_traffic_detail_statelog_employee] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_statelog';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_statelog';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_statelog';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_statelog', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_statelog', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_statelog', @level2type = N'COLUMN', @level2name = N'broadcast_traffic_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_statelog', @level2type = N'COLUMN', @level2name = N'broadcast_traffic_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_statelog', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_statelog', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_statelog', @level2type = N'COLUMN', @level2name = N'accepted';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_statelog', @level2type = N'COLUMN', @level2name = N'accepted';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_statelog', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_traffic_detail_statelog', @level2type = N'COLUMN', @level2name = N'employee_id';

