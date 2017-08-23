CREATE TABLE [dbo].[outlook_contact_emails] (
    [outlook_email_id]   INT NOT NULL,
    [outlook_contact_id] INT NOT NULL,
    CONSTRAINT [PK_outlook_contact_emails] PRIMARY KEY CLUSTERED ([outlook_email_id] ASC, [outlook_contact_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_outlook_contact_emails_contacts] FOREIGN KEY ([outlook_contact_id]) REFERENCES [dbo].[outlook_contacts] ([id]),
    CONSTRAINT [FK_outlook_contact_emails_emails] FOREIGN KEY ([outlook_email_id]) REFERENCES [dbo].[outlook_emails] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_emails';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_emails';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_emails';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_emails', @level2type = N'COLUMN', @level2name = N'outlook_email_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_emails', @level2type = N'COLUMN', @level2name = N'outlook_email_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_emails', @level2type = N'COLUMN', @level2name = N'outlook_contact_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contact_emails', @level2type = N'COLUMN', @level2name = N'outlook_contact_id';

