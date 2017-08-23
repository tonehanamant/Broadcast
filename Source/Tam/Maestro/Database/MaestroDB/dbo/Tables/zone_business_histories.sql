CREATE TABLE [dbo].[zone_business_histories] (
    [zone_id]     INT          NOT NULL,
    [business_id] INT          NOT NULL,
    [start_date]  DATETIME     NOT NULL,
    [type]        VARCHAR (15) NOT NULL,
    [end_date]    DATETIME     NOT NULL,
    CONSTRAINT [PK_zone_business_histories] PRIMARY KEY CLUSTERED ([zone_id] ASC, [business_id] ASC, [start_date] ASC, [type] ASC),
    CONSTRAINT [FK_zone_business_histories_businesses] FOREIGN KEY ([business_id]) REFERENCES [dbo].[businesses] ([id]),
    CONSTRAINT [FK_zone_business_histories_zones] FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_business_histories';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_business_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_business_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_business_histories', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_business_histories', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_business_histories', @level2type = N'COLUMN', @level2name = N'business_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_business_histories', @level2type = N'COLUMN', @level2name = N'business_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_business_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_business_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_business_histories', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_business_histories', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_business_histories', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zone_business_histories', @level2type = N'COLUMN', @level2name = N'end_date';

