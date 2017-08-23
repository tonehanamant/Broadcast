CREATE TABLE [dbo].[business_histories] (
    [business_id] INT          NOT NULL,
    [start_date]  DATETIME     NOT NULL,
    [code]        VARCHAR (15) NOT NULL,
    [name]        VARCHAR (63) NOT NULL,
    [type]        VARCHAR (15) NOT NULL,
    [active]      BIT          NOT NULL,
    [end_date]    DATETIME     NOT NULL,
    CONSTRAINT [PK_business_hitories] PRIMARY KEY CLUSTERED ([business_id] ASC, [start_date] ASC),
    CONSTRAINT [FK_business_histories_businesses] FOREIGN KEY ([business_id]) REFERENCES [dbo].[businesses] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains the change history of the businesses table.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Business History', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories', @level2type = N'COLUMN', @level2name = N'business_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories', @level2type = N'COLUMN', @level2name = N'business_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'business_histories', @level2type = N'COLUMN', @level2name = N'end_date';

