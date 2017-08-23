CREATE TABLE [dbo].[cmw_traffic_company_companies] (
    [child_cmw_traffic_company_id]  INT NOT NULL,
    [parent_cmw_traffic_company_id] INT NOT NULL,
    CONSTRAINT [PK_cmw_traffic_company_companies] PRIMARY KEY CLUSTERED ([child_cmw_traffic_company_id] ASC, [parent_cmw_traffic_company_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_cmw_traffic_company_companies_cmw_traffic_companies] FOREIGN KEY ([child_cmw_traffic_company_id]) REFERENCES [dbo].[cmw_traffic_companies] ([id]),
    CONSTRAINT [FK_cmw_traffic_company_companies_cmw_traffic_companies1] FOREIGN KEY ([parent_cmw_traffic_company_id]) REFERENCES [dbo].[cmw_traffic_companies] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_company_companies';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_company_companies';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_company_companies';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_company_companies', @level2type = N'COLUMN', @level2name = N'child_cmw_traffic_company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_company_companies', @level2type = N'COLUMN', @level2name = N'child_cmw_traffic_company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_company_companies', @level2type = N'COLUMN', @level2name = N'parent_cmw_traffic_company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_company_companies', @level2type = N'COLUMN', @level2name = N'parent_cmw_traffic_company_id';

