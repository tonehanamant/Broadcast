CREATE TABLE [dbo].[system_group_system_histories] (
    [system_group_id] INT      NOT NULL,
    [system_id]       INT      NOT NULL,
    [start_date]      DATETIME NOT NULL,
    [end_date]        DATETIME NOT NULL,
    CONSTRAINT [PK_system_group_system_histories] PRIMARY KEY CLUSTERED ([system_group_id] ASC, [system_id] ASC, [start_date] ASC),
    CONSTRAINT [FK_system_group_system_histories_system_groups] FOREIGN KEY ([system_group_id]) REFERENCES [dbo].[system_groups] ([id]),
    CONSTRAINT [FK_system_group_system_histories_systems] FOREIGN KEY ([system_id]) REFERENCES [dbo].[systems] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_group_system_histories';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'System Groups to Systems History', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_group_system_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_group_system_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the system group record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_group_system_histories', @level2type = N'COLUMN', @level2name = N'system_group_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_group_system_histories', @level2type = N'COLUMN', @level2name = N'system_group_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the system record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_group_system_histories', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_group_system_histories', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Starting date this data was accurate.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_group_system_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_group_system_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Ending date this data was accurate.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_group_system_histories', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_group_system_histories', @level2type = N'COLUMN', @level2name = N'end_date';

