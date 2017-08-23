CREATE TABLE [dbo].[system_statement_group_systems] (
    [system_statement_group_id] INT NOT NULL,
    [system_id]                 INT NOT NULL,
    CONSTRAINT [PK_system_statement_group_systems] PRIMARY KEY CLUSTERED ([system_statement_group_id] ASC, [system_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_system_statement_group_systems_system_statement_groups] FOREIGN KEY ([system_statement_group_id]) REFERENCES [dbo].[system_statement_groups] ([id]),
    CONSTRAINT [FK_system_statement_group_systems_systems] FOREIGN KEY ([system_id]) REFERENCES [dbo].[systems] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_group_systems';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_group_systems';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_group_systems';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_group_systems', @level2type = N'COLUMN', @level2name = N'system_statement_group_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_group_systems', @level2type = N'COLUMN', @level2name = N'system_statement_group_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_group_systems', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_group_systems', @level2type = N'COLUMN', @level2name = N'system_id';

