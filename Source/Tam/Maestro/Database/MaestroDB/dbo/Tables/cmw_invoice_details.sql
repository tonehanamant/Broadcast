CREATE TABLE [dbo].[cmw_invoice_details] (
    [cmw_invoice_id] INT NOT NULL,
    [cmw_traffic_id] INT NOT NULL,
    CONSTRAINT [PK_cmw_invoice_details] PRIMARY KEY CLUSTERED ([cmw_invoice_id] ASC, [cmw_traffic_id] ASC),
    CONSTRAINT [FK_cmw_invoice_details_cmw_invoices] FOREIGN KEY ([cmw_invoice_id]) REFERENCES [dbo].[cmw_invoices] ([id]),
    CONSTRAINT [FK_cmw_invoice_details_cmw_traffic] FOREIGN KEY ([cmw_traffic_id]) REFERENCES [dbo].[cmw_traffic] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_invoice_details';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_invoice_details';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_invoice_details';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_invoice_details', @level2type = N'COLUMN', @level2name = N'cmw_invoice_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_invoice_details', @level2type = N'COLUMN', @level2name = N'cmw_invoice_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_invoice_details', @level2type = N'COLUMN', @level2name = N'cmw_traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_invoice_details', @level2type = N'COLUMN', @level2name = N'cmw_traffic_id';

