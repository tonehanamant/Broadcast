CREATE TABLE [dbo].[outlook_contact_addresses] (
    [outlook_contact_id] INT NOT NULL,
    [outlook_address_id] INT NOT NULL,
    CONSTRAINT [PK_outlook_contact_addresses] PRIMARY KEY CLUSTERED ([outlook_contact_id] ASC, [outlook_address_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_outlook_contact_addresses_outlook_addresses] FOREIGN KEY ([outlook_address_id]) REFERENCES [dbo].[outlook_addresses] ([id]),
    CONSTRAINT [FK_outlook_contact_addresses_outlook_contacts] FOREIGN KEY ([outlook_contact_id]) REFERENCES [dbo].[outlook_contacts] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_addresses';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_addresses';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_addresses';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_addresses', @level2type = N'COLUMN', @level2name = N'outlook_contact_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_addresses', @level2type = N'COLUMN', @level2name = N'outlook_contact_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_addresses', @level2type = N'COLUMN', @level2name = N'outlook_address_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_addresses', @level2type = N'COLUMN', @level2name = N'outlook_address_id';

