CREATE TABLE [dbo].[great_plains_mapping] (
    [advertiser_id]                INT          NOT NULL,
    [product_id]                   INT          NOT NULL,
    [cmw_division_id]              INT          CONSTRAINT [DF_great_plains_mapping_cmw_division_id] DEFAULT ((1)) NOT NULL,
    [great_plains_customer_number] VARCHAR (8)  NULL,
    [advertiser_alias]             VARCHAR (63) NULL,
    [product_alias]                VARCHAR (63) NULL,
    CONSTRAINT [PK_great_plains_mapping] PRIMARY KEY CLUSTERED ([advertiser_id] ASC, [product_id] ASC, [cmw_division_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_great_plains_mapping_great_plains_customers] FOREIGN KEY ([great_plains_customer_number]) REFERENCES [dbo].[great_plains_customers] ([customer_number])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping', @level2type = N'COLUMN', @level2name = N'advertiser_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping', @level2type = N'COLUMN', @level2name = N'advertiser_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping', @level2type = N'COLUMN', @level2name = N'product_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping', @level2type = N'COLUMN', @level2name = N'product_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping', @level2type = N'COLUMN', @level2name = N'cmw_division_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping', @level2type = N'COLUMN', @level2name = N'cmw_division_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping', @level2type = N'COLUMN', @level2name = N'great_plains_customer_number';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping', @level2type = N'COLUMN', @level2name = N'great_plains_customer_number';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping', @level2type = N'COLUMN', @level2name = N'advertiser_alias';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping', @level2type = N'COLUMN', @level2name = N'advertiser_alias';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping', @level2type = N'COLUMN', @level2name = N'product_alias';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_mapping', @level2type = N'COLUMN', @level2name = N'product_alias';

