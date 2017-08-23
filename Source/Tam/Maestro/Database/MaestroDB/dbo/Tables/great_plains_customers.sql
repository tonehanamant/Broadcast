CREATE TABLE [dbo].[great_plains_customers] (
    [customer_number] VARCHAR (8)   NOT NULL,
    [customer_name]   VARCHAR (65)  NOT NULL,
    [Address1]        VARCHAR (128) NOT NULL,
    [Address2]        VARCHAR (64)  NULL,
    [City]            VARCHAR (50)  NOT NULL,
    [State]           CHAR (2)      NOT NULL,
    [Zip]             VARCHAR (10)  NOT NULL,
    [email]           VARCHAR (50)  NULL,
    [phone]           VARCHAR (25)  NULL,
    [date_modified]   SMALLDATETIME CONSTRAINT [DF_great_plains_customers_date_modified] DEFAULT (getdate()) NOT NULL,
    [modified_by]     VARCHAR (20)  CONSTRAINT [DF_great_plains_customers_modified_by] DEFAULT (suser_sname()) NOT NULL,
    [include_ISCI]    BIT           CONSTRAINT [DF_great_plains_customers_IncludeISCI] DEFAULT ((0)) NOT NULL,
    [cmw_division_id] INT           CONSTRAINT [DF_great_plains_customers_cmw_division_id] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_great_plains_customers] PRIMARY KEY CLUSTERED ([customer_number] ASC) WITH (FILLFACTOR = 90)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'customer_number';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'customer_number';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'customer_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'customer_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'Address1';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'Address1';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'Address2';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'Address2';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'City';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'City';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'State';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'State';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'Zip';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'Zip';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'email';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'email';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'phone';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'phone';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'date_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'date_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'modified_by';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'modified_by';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'include_ISCI';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'include_ISCI';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'cmw_division_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'great_plains_customers', @level2type = N'COLUMN', @level2name = N'cmw_division_id';

