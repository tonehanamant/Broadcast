CREATE TABLE [dbo].[cmw_traffic_agencies] (
    [cmw_traffic_company_id] INT NOT NULL,
    CONSTRAINT [PK_cmw_traffic_agencies_1] PRIMARY KEY CLUSTERED ([cmw_traffic_company_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_cmw_traffic_agencies_cmw_traffic_companies] FOREIGN KEY ([cmw_traffic_company_id]) REFERENCES [dbo].[cmw_traffic_companies] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_agencies';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_agencies';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_agencies';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_agencies', @level2type = N'COLUMN', @level2name = N'cmw_traffic_company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_traffic_agencies', @level2type = N'COLUMN', @level2name = N'cmw_traffic_company_id';

