CREATE TABLE [dbo].[contact_addresses] (
    [contact_id] INT NOT NULL,
    [address_id] INT NOT NULL,
    CONSTRAINT [PK_contact_addresses] PRIMARY KEY CLUSTERED ([contact_id] ASC, [address_id] ASC),
    CONSTRAINT [FK_contact_addresses_addresses] FOREIGN KEY ([address_id]) REFERENCES [dbo].[addresses] ([id]),
    CONSTRAINT [FK_contact_addresses_contacts] FOREIGN KEY ([contact_id]) REFERENCES [dbo].[contacts] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_addresses';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_addresses';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_addresses';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_addresses', @level2type = N'COLUMN', @level2name = N'contact_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_addresses', @level2type = N'COLUMN', @level2name = N'contact_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_addresses', @level2type = N'COLUMN', @level2name = N'address_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_addresses', @level2type = N'COLUMN', @level2name = N'address_id';

