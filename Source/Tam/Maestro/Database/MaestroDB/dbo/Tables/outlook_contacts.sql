CREATE TABLE [dbo].[outlook_contacts] (
    [id]                 INT           IDENTITY (1, 1) NOT NULL,
    [outlook_export_id]  INT           NOT NULL,
    [outlook_company_id] INT           NULL,
    [salutation_id]      INT           NULL,
    [first_name]         VARCHAR (63)  NOT NULL,
    [last_name]          VARCHAR (63)  NOT NULL,
    [title]              VARCHAR (63)  NOT NULL,
    [department]         VARCHAR (63)  NOT NULL,
    [assistant]          VARCHAR (63)  NOT NULL,
    [assistant_title]    VARCHAR (63)  NOT NULL,
    [web_page_address]   VARCHAR (512) NOT NULL,
    [im_address]         VARCHAR (512) NOT NULL,
    CONSTRAINT [PK_outlook_contacts] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_outlook_contacts_outlook_companies] FOREIGN KEY ([outlook_company_id]) REFERENCES [dbo].[outlook_companies] ([id]),
    CONSTRAINT [FK_outlook_contacts_outlook_exports] FOREIGN KEY ([outlook_export_id]) REFERENCES [dbo].[outlook_exports] ([id]),
    CONSTRAINT [FK_outlook_contacts_salutations] FOREIGN KEY ([salutation_id]) REFERENCES [dbo].[salutations] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'outlook_export_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'outlook_export_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'outlook_company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'outlook_company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'salutation_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'salutation_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'first_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'first_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'last_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'last_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'title';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'title';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'department';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'department';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'assistant';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'assistant';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'assistant_title';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'assistant_title';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'web_page_address';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'web_page_address';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'im_address';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'outlook_contacts', @level2type = N'COLUMN', @level2name = N'im_address';

