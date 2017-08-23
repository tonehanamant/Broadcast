CREATE TABLE [dbo].[company_companies] (
    [company_id]        INT NOT NULL,
    [parent_company_id] INT NOT NULL,
    CONSTRAINT [PK_company_companies_1] PRIMARY KEY CLUSTERED ([company_id] ASC, [parent_company_id] ASC),
    CONSTRAINT [FK_company_companies_companies] FOREIGN KEY ([company_id]) REFERENCES [dbo].[companies] ([id]),
    CONSTRAINT [FK_company_companies_companies1] FOREIGN KEY ([parent_company_id]) REFERENCES [dbo].[companies] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'company_companies';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'company_companies';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'company_companies';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'company_companies', @level2type = N'COLUMN', @level2name = N'company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'company_companies', @level2type = N'COLUMN', @level2name = N'company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'company_companies', @level2type = N'COLUMN', @level2name = N'parent_company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'company_companies', @level2type = N'COLUMN', @level2name = N'parent_company_id';

