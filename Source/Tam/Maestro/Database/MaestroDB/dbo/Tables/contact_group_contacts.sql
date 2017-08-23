CREATE TABLE [dbo].[contact_group_contacts] (
    [contact_id]       INT NOT NULL,
    [contact_group_id] INT NOT NULL,
    CONSTRAINT [PK_contact_group_contacts] PRIMARY KEY CLUSTERED ([contact_id] ASC, [contact_group_id] ASC),
    CONSTRAINT [FK_contact_group_contacts_contact_groups] FOREIGN KEY ([contact_group_id]) REFERENCES [dbo].[contact_groups] ([id]),
    CONSTRAINT [FK_contact_group_contacts_contacts] FOREIGN KEY ([contact_id]) REFERENCES [dbo].[contacts] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_group_contacts';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_group_contacts';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_group_contacts';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_group_contacts', @level2type = N'COLUMN', @level2name = N'contact_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_group_contacts', @level2type = N'COLUMN', @level2name = N'contact_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_group_contacts', @level2type = N'COLUMN', @level2name = N'contact_group_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_group_contacts', @level2type = N'COLUMN', @level2name = N'contact_group_id';

