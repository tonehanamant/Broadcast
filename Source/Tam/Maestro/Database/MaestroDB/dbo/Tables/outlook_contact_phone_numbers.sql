CREATE TABLE [dbo].[outlook_contact_phone_numbers] (
    [outlook_contact_id]      INT NOT NULL,
    [outlook_phone_number_id] INT NOT NULL,
    CONSTRAINT [PK_outlook_contact_phone_numbers] PRIMARY KEY CLUSTERED ([outlook_contact_id] ASC, [outlook_phone_number_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_outlook_contact_phone_numbers_outlook_contacts] FOREIGN KEY ([outlook_contact_id]) REFERENCES [dbo].[outlook_contacts] ([id]),
    CONSTRAINT [FK_outlook_contact_phone_numbers_outlook_phone_numbers] FOREIGN KEY ([outlook_phone_number_id]) REFERENCES [dbo].[outlook_phone_numbers] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_phone_numbers';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_phone_numbers';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_phone_numbers';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_phone_numbers', @level2type = N'COLUMN', @level2name = N'outlook_contact_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_phone_numbers', @level2type = N'COLUMN', @level2name = N'outlook_contact_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_phone_numbers', @level2type = N'COLUMN', @level2name = N'outlook_phone_number_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_phone_numbers', @level2type = N'COLUMN', @level2name = N'outlook_phone_number_id';

