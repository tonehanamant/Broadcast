CREATE TABLE [dbo].[topography_system_group_histories] (
    [topography_id]   INT      NOT NULL,
    [system_group_id] INT      NOT NULL,
    [start_date]      DATETIME NOT NULL,
    [include]         BIT      NOT NULL,
    [end_date]        DATETIME NOT NULL,
    CONSTRAINT [PK_topography_system_group_histories] PRIMARY KEY CLUSTERED ([topography_id] ASC, [system_group_id] ASC, [start_date] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_topography_system_group_histories_system_groups] FOREIGN KEY ([system_group_id]) REFERENCES [dbo].[system_groups] ([id]),
    CONSTRAINT [FK_topography_system_group_histories_topographies] FOREIGN KEY ([topography_id]) REFERENCES [dbo].[topographies] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_group_histories';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_group_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_group_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_group_histories', @level2type = N'COLUMN', @level2name = N'topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_group_histories', @level2type = N'COLUMN', @level2name = N'topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_group_histories', @level2type = N'COLUMN', @level2name = N'system_group_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_group_histories', @level2type = N'COLUMN', @level2name = N'system_group_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_group_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_group_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_group_histories', @level2type = N'COLUMN', @level2name = N'include';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_group_histories', @level2type = N'COLUMN', @level2name = N'include';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_group_histories', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_group_histories', @level2type = N'COLUMN', @level2name = N'end_date';

