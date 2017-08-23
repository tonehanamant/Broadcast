CREATE TABLE [dbo].[businesses] (
    [id]             INT          IDENTITY (1, 1) NOT NULL,
    [code]           VARCHAR (15) NOT NULL,
    [name]           VARCHAR (63) NOT NULL,
    [type]           VARCHAR (15) NOT NULL,
    [active]         BIT          NOT NULL,
    [effective_date] DATETIME     NOT NULL,
    CONSTRAINT [PK_business] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains a list of the entities with which we do business with.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Businesses', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'This incluides MSO''s.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Short name.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Long name.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Type', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'MSO', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Whether or not this record is active.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'1=Yes, 0=No', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Starting date this data is accurate.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'businesses', @level2type = N'COLUMN', @level2name = N'effective_date';

