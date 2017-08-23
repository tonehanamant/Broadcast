CREATE TABLE [dbo].[invoices] (
    [id]                        INT          IDENTITY (1, 1) NOT NULL,
    [system_id]                 INT          NULL,
    [invoicing_system_id]       INT          NULL,
    [affidavit_file_id]         INT          NOT NULL,
    [media_month_id]            INT          NOT NULL,
    [external_id]               VARCHAR (15) NOT NULL,
    [external_id_suffix]        VARCHAR (15) NOT NULL,
    [invoice_date]              DATETIME     NOT NULL,
    [invoice_gross_due]         INT          NOT NULL,
    [invoice_spot_num]          INT          NOT NULL,
    [invoice_estimate_code]     VARCHAR (63) NOT NULL,
    [invoice_con_start_date]    DATETIME     NOT NULL,
    [invoice_con_end_date]      DATETIME     NOT NULL,
    [invoice_external_id]       VARCHAR (15) NOT NULL,
    [invoice_system_name]       VARCHAR (63) NOT NULL,
    [invoice_media_month]       VARCHAR (15) NOT NULL,
    [date_created]              DATETIME     NOT NULL,
    [invoice_net_due]           INT          NULL,
    [invoice_agency_commission] INT          NULL,
    [address_line_1]            VARCHAR (63) CONSTRAINT [DF_invoices_address_line_1] DEFAULT ('') NOT NULL,
    [address_line_2]            VARCHAR (63) CONSTRAINT [DF_invoices_address_line_2] DEFAULT ('') NOT NULL,
    [address_line_3]            VARCHAR (63) CONSTRAINT [DF_invoices_address_line_3] DEFAULT ('') NOT NULL,
    [address_line_4]            VARCHAR (63) CONSTRAINT [DF_invoices_address_line_4] DEFAULT ('') NOT NULL,
    [address_line_5]            VARCHAR (63) CONSTRAINT [DF_invoices_address_line_5] DEFAULT ('') NOT NULL,
    [computer_system]           VARCHAR (15) CONSTRAINT [DF_invoices_computer_system] DEFAULT ('') NOT NULL,
    [business_id]               INT          NULL,
    CONSTRAINT [PK_invoices] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_invoices_affidavit_files] FOREIGN KEY ([affidavit_file_id]) REFERENCES [dbo].[affidavit_files] ([id]),
    CONSTRAINT [FK_invoices_businesses] FOREIGN KEY ([business_id]) REFERENCES [dbo].[businesses] ([id]),
    CONSTRAINT [FK_invoices_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_invoices_systems] FOREIGN KEY ([system_id]) REFERENCES [dbo].[systems] ([id]),
    CONSTRAINT [FK_invoices_systems1] FOREIGN KEY ([invoicing_system_id]) REFERENCES [dbo].[systems] ([id])
);


GO
CREATE NONCLUSTERED INDEX [Ind_invoices_media_month_id]
    ON [dbo].[invoices]([media_month_id] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_invoices_system_id]
    ON [dbo].[invoices]([system_id] ASC)
    INCLUDE([media_month_id]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_system_by_month_and_file]
    ON [dbo].[invoices]([media_month_id] ASC, [affidavit_file_id] ASC)
    INCLUDE([system_id], [invoicing_system_id]);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains the invoices received from the electronic affidavit files. One or more affidavits make up an invoice.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Invoices', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Use this table for determining the system and invoice number of an affidavit.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the system record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'This refers to the system at the time the invoice was imported; it is therefore date sensitive.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the system record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoicing_system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'This refers to the invoicing system at the time the invoice was imported; it is therefore date sensitive. Checks are sent out based on this system.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoicing_system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the affidavit file record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'affidavit_file_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'What file the invoice came from.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'affidavit_file_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Invoice number', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'external_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'This is the invoice number minue the invoice number suffix.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'external_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Invoice number suffix.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'external_id_suffix';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'This is the letter or number following an invoice number. Generally this is inserted to indicate the same invoice from a different zone.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'external_id_suffix';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Date of the invoice.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Pulled from invoice.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Total gross due from invoice footer.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_gross_due';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Pulled from invoice.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_gross_due';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Total number of spots from invoice footer.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_spot_num';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Pulled from invoice.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_spot_num';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Agency estimate code', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_estimate_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Pulled from invoice. This should contain our traffic order identifier.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_estimate_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contract start date.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_con_start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Pulled from invoice.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_con_start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contract end date.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_con_end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Pulled from invoice.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_con_end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Original, unmodified invoice number.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_external_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Pulled from invoice.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_external_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'System name.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_system_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Pulled from invoice.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_system_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N' Media month.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_media_month';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Pulled from invoice, but flipped to be in the format MMYY.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_media_month';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_net_due';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_net_due';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_agency_commission';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'invoice_agency_commission';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'address_line_1';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'address_line_1';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'address_line_2';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'address_line_2';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'address_line_3';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'address_line_3';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'address_line_4';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'address_line_4';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'address_line_5';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'address_line_5';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'computer_system';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'computer_system';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'business_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'invoices', @level2type = N'COLUMN', @level2name = N'business_id';

