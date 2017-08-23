CREATE TABLE [dbo].[cmw_bill_adjustments] (
    [cmw_bill_id]               INT NOT NULL,
    [cmw_invoice_adjustment_id] INT NOT NULL,
    CONSTRAINT [PK_cmw_bill_adjustments] PRIMARY KEY CLUSTERED ([cmw_bill_id] ASC, [cmw_invoice_adjustment_id] ASC) WITH (FILLFACTOR = 90)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_bill_adjustments';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_bill_adjustments';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_bill_adjustments';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_bill_adjustments', @level2type = N'COLUMN', @level2name = N'cmw_bill_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_bill_adjustments', @level2type = N'COLUMN', @level2name = N'cmw_bill_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_bill_adjustments', @level2type = N'COLUMN', @level2name = N'cmw_invoice_adjustment_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cmw_bill_adjustments', @level2type = N'COLUMN', @level2name = N'cmw_invoice_adjustment_id';

