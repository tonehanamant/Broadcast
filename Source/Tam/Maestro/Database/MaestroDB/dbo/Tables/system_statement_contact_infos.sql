CREATE TABLE [dbo].[system_statement_contact_infos] (
    [id]                        INT           IDENTITY (1, 1) NOT NULL,
    [system_statement_group_id] INT           NULL,
    [system_id]                 INT           NULL,
    [first_name]                VARCHAR (63)  NOT NULL,
    [last_name]                 VARCHAR (63)  NOT NULL,
    [email_address]             VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_system_statement_contact_infos] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_system_statement_contact_infos_systems] FOREIGN KEY ([system_id]) REFERENCES [dbo].[systems] ([id]),
    CONSTRAINT [FK_system_statement_group_contact_infos_system_statement_groups] FOREIGN KEY ([system_statement_group_id]) REFERENCES [dbo].[system_statement_groups] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos', @level2type = N'COLUMN', @level2name = N'system_statement_group_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos', @level2type = N'COLUMN', @level2name = N'system_statement_group_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos', @level2type = N'COLUMN', @level2name = N'first_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos', @level2type = N'COLUMN', @level2name = N'first_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos', @level2type = N'COLUMN', @level2name = N'last_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos', @level2type = N'COLUMN', @level2name = N'last_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos', @level2type = N'COLUMN', @level2name = N'email_address';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_statement_contact_infos', @level2type = N'COLUMN', @level2name = N'email_address';

