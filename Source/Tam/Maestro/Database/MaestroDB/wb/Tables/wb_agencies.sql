CREATE TABLE [wb].[wb_agencies] (
    [id]           INT           IDENTITY (1, 1) NOT NULL,
    [code]         VARCHAR (127) NOT NULL,
    [name]         VARCHAR (127) NOT NULL,
    [address]      VARCHAR (127) NULL,
    [city]         VARCHAR (127) NULL,
    [state]        VARCHAR (127) NULL,
    [zipcode]      VARCHAR (127) NULL,
    [phone_number] VARCHAR (127) NULL,
    [contact]      VARCHAR (127) NULL,
    CONSTRAINT [PK_wb_agencies] PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'address';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'address';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'city';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'city';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'state';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'state';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'zipcode';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'zipcode';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'phone_number';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'phone_number';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'contact';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'wb', @level1type = N'TABLE', @level1name = N'wb_agencies', @level2type = N'COLUMN', @level2name = N'contact';

