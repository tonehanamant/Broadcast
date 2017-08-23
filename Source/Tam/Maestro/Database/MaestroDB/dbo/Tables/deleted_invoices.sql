CREATE TABLE [dbo].[deleted_invoices] (
    [id]                        INT          NOT NULL,
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
    [address_line_1]            VARCHAR (63) CONSTRAINT [DF_deleted_invoices_address_line_1] DEFAULT ('') NOT NULL,
    [address_line_2]            VARCHAR (63) CONSTRAINT [DF_deleted_invoices_address_line_2] DEFAULT ('') NOT NULL,
    [address_line_3]            VARCHAR (63) CONSTRAINT [DF_deleted_invoices_address_line_3] DEFAULT ('') NOT NULL,
    [address_line_4]            VARCHAR (63) CONSTRAINT [DF_deleted_invoices_address_line_4] DEFAULT ('') NOT NULL,
    [address_line_5]            VARCHAR (63) CONSTRAINT [DF_deleted_invoices_address_line_5] DEFAULT ('') NOT NULL,
    [computer_system]           VARCHAR (15) CONSTRAINT [DF_deleted_invoices_computer_system] DEFAULT ('') NOT NULL,
    [business_id]               INT          NULL,
    CONSTRAINT [PK_deleted_invoices] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoicing_system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoicing_system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'affidavit_file_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'affidavit_file_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'external_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'external_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'external_id_suffix';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'external_id_suffix';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_gross_due';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_gross_due';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_spot_num';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_spot_num';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_estimate_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_estimate_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_con_start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_con_start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_con_end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_con_end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_external_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_external_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_system_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_system_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_media_month';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_media_month';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_net_due';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_net_due';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_agency_commission';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'invoice_agency_commission';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'address_line_1';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'address_line_1';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'address_line_2';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'address_line_2';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'address_line_3';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'address_line_3';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'address_line_4';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'address_line_4';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'address_line_5';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'address_line_5';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'computer_system';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'computer_system';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'business_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'deleted_invoices', @level2type = N'COLUMN', @level2name = N'business_id';

