CREATE TABLE [dbo].[products] (
    [id]                        INT            IDENTITY (1, 1) NOT NULL,
    [name]                      VARCHAR (127)  NOT NULL,
    [description]               VARCHAR (255)  NULL,
    [company_id]                INT            NOT NULL,
    [default_rate_card_type_id] INT            NOT NULL,
    [geo_sensitive_comment]     VARCHAR (2047) NULL,
    [pol_sensitive_comment]     VARCHAR (2047) NULL,
    [date_created]              DATETIME       NOT NULL,
    [date_last_modified]        DATETIME       NOT NULL,
    [display_name]              VARCHAR (63)   CONSTRAINT [DF_products_display_name] DEFAULT ('') NULL,
    CONSTRAINT [PK_products] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_products_companies] FOREIGN KEY ([company_id]) REFERENCES [dbo].[companies] ([id]),
    CONSTRAINT [FK_products_rate_card_types] FOREIGN KEY ([default_rate_card_type_id]) REFERENCES [dbo].[rate_card_types] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Products', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'description';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'description';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'default_rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'default_rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'geo_sensitive_comment';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'geo_sensitive_comment';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'pol_sensitive_comment';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'pol_sensitive_comment';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'date_last_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'date_last_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'display_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'products', @level2type = N'COLUMN', @level2name = N'display_name';

