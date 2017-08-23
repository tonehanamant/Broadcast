CREATE TABLE [dbo].[employee_roles] (
    [role_id]     INT NOT NULL,
    [employee_id] INT NOT NULL,
    CONSTRAINT [PK_employee_roles] PRIMARY KEY CLUSTERED ([role_id] ASC, [employee_id] ASC),
    CONSTRAINT [FK_employee_roles_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_employee_roles_roles] FOREIGN KEY ([role_id]) REFERENCES [dbo].[roles] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employee_roles';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employee_roles';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employee_roles';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employee_roles', @level2type = N'COLUMN', @level2name = N'role_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employee_roles', @level2type = N'COLUMN', @level2name = N'role_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employee_roles', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employee_roles', @level2type = N'COLUMN', @level2name = N'employee_id';

