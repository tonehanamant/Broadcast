CREATE TABLE [dbo].[sales_models] (
    [id]                                 INT           IDENTITY (1, 1) NOT NULL,
    [name]                               VARCHAR (63)  NOT NULL,
    [code]                               VARCHAR (15)  CONSTRAINT [DF_sales_models_code] DEFAULT ('') NOT NULL,
    [display_name]                       VARCHAR (31)  CONSTRAINT [DF_sales_models_display_name] DEFAULT ('') NOT NULL,
    [scx_name]                           VARCHAR (127) CONSTRAINT [DF_sales_models_scx_name] DEFAULT ('') NOT NULL,
    [scx_office]                         VARCHAR (127) CONSTRAINT [DF_sales_models_scx_office] DEFAULT ('') NOT NULL,
    [scx_ncc_universal_agency_office_id] VARCHAR (15)  CONSTRAINT [DF_sales_models_scx_ncc_universal_agency_office_id] DEFAULT ('') NOT NULL,
    [scx_street]                         VARCHAR (255) CONSTRAINT [DF_sales_models_scx_street] DEFAULT ('') NOT NULL,
    [scx_country]                        VARCHAR (63)  CONSTRAINT [DF_sales_models_scx_country] DEFAULT ('') NOT NULL,
    [scx_city]                           VARCHAR (63)  CONSTRAINT [DF_sales_models_scx_city] DEFAULT ('') NOT NULL,
    [scx_state]                          VARCHAR (63)  CONSTRAINT [DF_sales_models_scx_state] DEFAULT ('') NOT NULL,
    [scx_zip]                            VARCHAR (15)  CONSTRAINT [DF_sales_models_scx_zip] DEFAULT ('') NOT NULL,
    [scx_contact_first_name]             VARCHAR (63)  CONSTRAINT [DF_sales_models_scx_contact_first_name] DEFAULT ('') NOT NULL,
    [scx_contact_last_name]              VARCHAR (63)  CONSTRAINT [DF_sales_models_scx_contact_last_name] DEFAULT ('') NOT NULL,
    [scx_contact_email]                  VARCHAR (255) CONSTRAINT [DF_sales_models_scx_contact_email] DEFAULT ('') NOT NULL,
    [scx_contact_phone]                  VARCHAR (31)  CONSTRAINT [DF_sales_models_scx_contact_phone] DEFAULT ('') NOT NULL,
    [scx_contact_fax]                    VARCHAR (31)  CONSTRAINT [DF_sales_models_scx_contact_fax] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_sales_models] PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'display_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'display_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_office';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_office';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_ncc_universal_agency_office_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_ncc_universal_agency_office_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_street';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_street';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_country';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_country';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_city';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_city';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_state';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_state';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_zip';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_zip';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_contact_first_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_contact_first_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_contact_last_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_contact_last_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_contact_email';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_contact_email';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_contact_phone';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_contact_phone';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_contact_fax';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_models', @level2type = N'COLUMN', @level2name = N'scx_contact_fax';

