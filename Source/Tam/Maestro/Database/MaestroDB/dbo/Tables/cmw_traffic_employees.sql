CREATE TABLE [dbo].[cmw_traffic_employees] (
    [cmw_traffic_id] INT      NOT NULL,
    [employee_id]    INT      NOT NULL,
    [effective_date] DATETIME NOT NULL,
    CONSTRAINT [PK_cmw_traffic_employees] PRIMARY KEY CLUSTERED ([cmw_traffic_id] ASC, [employee_id] ASC, [effective_date] ASC),
    CONSTRAINT [FK_cmw_traffic_employees_cmw_traffic] FOREIGN KEY ([cmw_traffic_id]) REFERENCES [dbo].[cmw_traffic] ([id]),
    CONSTRAINT [FK_cmw_traffic_employees_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_employees';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_employees';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_employees';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_employees', @level2type = N'COLUMN', @level2name = N'cmw_traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_employees', @level2type = N'COLUMN', @level2name = N'cmw_traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_employees', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_employees', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_employees', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_employees', @level2type = N'COLUMN', @level2name = N'effective_date';

