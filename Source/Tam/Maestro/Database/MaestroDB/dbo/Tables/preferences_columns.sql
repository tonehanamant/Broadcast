CREATE TABLE [dbo].[preferences_columns] (
    [employee_id]       INT     NOT NULL,
    [application_code]  TINYINT NOT NULL,
    [object_class_code] TINYINT NOT NULL,
    [column_code]       TINYINT NOT NULL,
    [visible]           BIT     NOT NULL,
    [sort_order]        TINYINT NOT NULL,
    [column_order]      TINYINT NOT NULL,
    [column_width]      INT     NOT NULL,
    CONSTRAINT [PK_preferences_columns] PRIMARY KEY CLUSTERED ([employee_id] ASC, [application_code] ASC, [object_class_code] ASC, [column_code] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_preferences_columns_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'application_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'application_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'object_class_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'object_class_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'column_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'column_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'visible';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'visible';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'sort_order';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'sort_order';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'column_order';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'column_order';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'column_width';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'preferences_columns', @level2type = N'COLUMN', @level2name = N'column_width';

