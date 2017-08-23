CREATE TABLE [dbo].[contact_phone_numbers] (
    [contact_id]      INT NOT NULL,
    [phone_number_id] INT NOT NULL,
    CONSTRAINT [PK_contact_phone_numbers] PRIMARY KEY CLUSTERED ([contact_id] ASC, [phone_number_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_contact_phone_numbers_contacts] FOREIGN KEY ([contact_id]) REFERENCES [dbo].[contacts] ([id]),
    CONSTRAINT [FK_contact_phone_numbers_phone_numbers] FOREIGN KEY ([phone_number_id]) REFERENCES [dbo].[phone_numbers] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_phone_numbers';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_phone_numbers';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_phone_numbers';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_phone_numbers', @level2type = N'COLUMN', @level2name = N'contact_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_phone_numbers', @level2type = N'COLUMN', @level2name = N'contact_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_phone_numbers', @level2type = N'COLUMN', @level2name = N'phone_number_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'contact_phone_numbers', @level2type = N'COLUMN', @level2name = N'phone_number_id';

