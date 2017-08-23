CREATE TABLE [dbo].[companies] (
    [id]                        INT            IDENTITY (1, 1) NOT NULL,
    [name]                      VARCHAR (63)   NOT NULL,
    [url]                       VARCHAR (127)  NULL,
    [company_status_id]         INT            NOT NULL,
    [salesperson_employee_id]   INT            NULL,
    [geo_sensitive_comment]     VARCHAR (2047) NULL,
    [pol_sensitive_comment]     VARCHAR (2047) NULL,
    [additional_information]    TEXT           NULL,
    [enabled]                   BIT            CONSTRAINT [DF_companies_is_active] DEFAULT ((1)) NOT NULL,
    [account_status_id]         INT            NULL,
    [default_rate_card_type_id] INT            NOT NULL,
    [default_billing_terms_id]  INT            CONSTRAINT [DF_companies_billing_terms_id] DEFAULT ((3)) NOT NULL,
    [date_created]              DATETIME       NOT NULL,
    [date_last_modified]        DATETIME       CONSTRAINT [DF_companies_last_changed] DEFAULT (getdate()) NOT NULL,
    [display_name]              VARCHAR (63)   CONSTRAINT [DF_companies_display_name] DEFAULT ('') NULL,
    [default_is_msa]            BIT            NULL,
    [agency_dds_idb_number]     VARCHAR (31)   CONSTRAINT [DF_companies_agency_dds_idb_number] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_companies] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_companies_account_statuses] FOREIGN KEY ([account_status_id]) REFERENCES [dbo].[account_statuses] ([id]),
    CONSTRAINT [FK_companies_billing_terms] FOREIGN KEY ([default_billing_terms_id]) REFERENCES [dbo].[billing_terms] ([id]),
    CONSTRAINT [FK_companies_company_statuses] FOREIGN KEY ([company_status_id]) REFERENCES [dbo].[company_statuses] ([id]),
    CONSTRAINT [FK_companies_employees] FOREIGN KEY ([salesperson_employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_companies_rate_card_types] FOREIGN KEY ([default_rate_card_type_id]) REFERENCES [dbo].[rate_card_types] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'url';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'url';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'company_status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'company_status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'salesperson_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'salesperson_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'geo_sensitive_comment';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'geo_sensitive_comment';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'pol_sensitive_comment';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'pol_sensitive_comment';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'additional_information';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'additional_information';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'enabled';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'enabled';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'account_status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'account_status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'default_rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'default_rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'default_billing_terms_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'default_billing_terms_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'date_last_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'date_last_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'display_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'display_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'default_is_msa';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'default_is_msa';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'agency_dds_idb_number';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'companies', @level2type = N'COLUMN', @level2name = N'agency_dds_idb_number';

