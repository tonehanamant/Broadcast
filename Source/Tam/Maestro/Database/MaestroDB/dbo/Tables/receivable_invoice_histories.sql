CREATE TABLE [dbo].[receivable_invoice_histories] (
    [receivable_invoice_id] INT           NOT NULL,
    [start_date]            DATETIME      NOT NULL,
    [media_month_id]        INT           NOT NULL,
    [entity_id]             INT           NOT NULL,
    [customer_number]       VARCHAR (8)   NULL,
    [invoice_number]        VARCHAR (63)  NOT NULL,
    [special_notes]         VARCHAR (MAX) NULL,
    [total_units]           INT           NOT NULL,
    [total_due_gross]       MONEY         NOT NULL,
    [total_due_net]         MONEY         NOT NULL,
    [total_credits]         MONEY         NOT NULL,
    [document_id]           INT           NULL,
    [is_mailed]             BIT           CONSTRAINT [DF_receivable_invoice_histories_is_mailed] DEFAULT ((0)) NOT NULL,
    [ISCI_codes]            VARCHAR (100) NULL,
    [active]                BIT           CONSTRAINT [DF_receivable_invoice_histories_active] DEFAULT ((1)) NOT NULL,
    [effective_date]        DATETIME      NOT NULL,
    [date_created]          DATETIME      NOT NULL,
    [date_modified]         DATETIME      NOT NULL,
    [modified_by]           VARCHAR (50)  NOT NULL,
    [end_date]              DATETIME      NOT NULL,
    [invoice_type_id]       INT           DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_receivable_invoice_histories_new] PRIMARY KEY CLUSTERED ([start_date] ASC, [media_month_id] ASC, [entity_id] ASC, [active] ASC),
    CONSTRAINT [FK_receivable_invoice_histories_documents] FOREIGN KEY ([document_id]) REFERENCES [dbo].[documents] ([id]),
    CONSTRAINT [FK_receivable_invoice_histories_great_plains_customers] FOREIGN KEY ([customer_number]) REFERENCES [dbo].[great_plains_customers] ([customer_number]),
    CONSTRAINT [FK_receivable_invoice_histories_invoice_types] FOREIGN KEY ([invoice_type_id]) REFERENCES [dbo].[invoice_types] ([id]),
    CONSTRAINT [FK_receivable_invoice_histories_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'receivable_invoice_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'receivable_invoice_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'entity_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'entity_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'customer_number';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'customer_number';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'invoice_number';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'invoice_number';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'special_notes';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'special_notes';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'total_units';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'total_units';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'total_due_gross';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'total_due_gross';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'total_due_net';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'total_due_net';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'total_credits';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'total_credits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'document_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'document_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'is_mailed';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'is_mailed';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'ISCI_codes';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'ISCI_codes';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'date_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'date_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'modified_by';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'modified_by';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'invoice_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'receivable_invoice_histories', @level2type = N'COLUMN', @level2name = N'invoice_type_id';

