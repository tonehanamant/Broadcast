CREATE TABLE [dbo].[system_zone_histories] (
    [zone_id]    INT          NOT NULL,
    [system_id]  INT          NOT NULL,
    [start_date] DATETIME     NOT NULL,
    [type]       VARCHAR (15) NOT NULL,
    [end_date]   DATETIME     NOT NULL,
    CONSTRAINT [PK_system_zone_histories] PRIMARY KEY CLUSTERED ([zone_id] ASC, [system_id] ASC, [start_date] ASC, [type] ASC),
    CONSTRAINT [FK_system_zone_histories_systems] FOREIGN KEY ([system_id]) REFERENCES [dbo].[systems] ([id]),
    CONSTRAINT [FK_system_zone_histories_zones] FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains the change history of the system_zones table.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_zone_histories';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Systems to Zones History', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_zone_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_zone_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the zone record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_zone_histories', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_zone_histories', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the system record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_zone_histories', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_zone_histories', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Starting date this data was accurate.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_zone_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_zone_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Traffic or Billing', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_zone_histories', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_zone_histories', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Ending date this data was accurate.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_zone_histories', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_zone_histories', @level2type = N'COLUMN', @level2name = N'end_date';

