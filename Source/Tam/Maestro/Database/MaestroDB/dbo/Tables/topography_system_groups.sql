CREATE TABLE [dbo].[topography_system_groups] (
    [topography_id]   INT      NOT NULL,
    [system_group_id] INT      NOT NULL,
    [include]         BIT      NOT NULL,
    [effective_date]  DATETIME NOT NULL,
    CONSTRAINT [PK_topography_system_groups] PRIMARY KEY CLUSTERED ([topography_id] ASC, [system_group_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_topography_system_groups_system_groups] FOREIGN KEY ([system_group_id]) REFERENCES [dbo].[system_groups] ([id]),
    CONSTRAINT [FK_topography_system_groups_topographies] FOREIGN KEY ([topography_id]) REFERENCES [dbo].[topographies] ([id])
);


GO
CREATE TRIGGER tr_topography_system_groups 
	ON dbo.topography_system_groups 
	AFTER INSERT 
AS 

SET NOCOUNT ON 

--insert trigger event 
INSERT INTO trigger_status (id, table_name, trigger_name, trigger_start_time, trigger_flag) 
VALUES(16, 'topography_system_groups', 'tr_topography_system_groups', GETDATE(), 1)

GO
DISABLE TRIGGER [dbo].[tr_topography_system_groups]
    ON [dbo].[topography_system_groups];


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_groups';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_groups';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_groups';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_groups', @level2type = N'COLUMN', @level2name = N'topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_groups', @level2type = N'COLUMN', @level2name = N'topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_groups', @level2type = N'COLUMN', @level2name = N'system_group_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_groups', @level2type = N'COLUMN', @level2name = N'system_group_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_groups', @level2type = N'COLUMN', @level2name = N'include';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_groups', @level2type = N'COLUMN', @level2name = N'include';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_groups', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_system_groups', @level2type = N'COLUMN', @level2name = N'effective_date';

