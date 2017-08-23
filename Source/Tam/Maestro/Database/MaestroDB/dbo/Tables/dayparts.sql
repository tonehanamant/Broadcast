CREATE TABLE [dbo].[dayparts] (
    [id]           INT          IDENTITY (1, 1) NOT NULL,
    [timespan_id]  INT          NOT NULL,
    [code]         VARCHAR (15) CONSTRAINT [DF_dayparts_code] DEFAULT ('CUS') NOT NULL,
    [name]         VARCHAR (63) CONSTRAINT [DF_dayparts_name] DEFAULT ('Custom') NOT NULL,
    [tier]         INT          CONSTRAINT [DF_dayparts_tier] DEFAULT ((0)) NOT NULL,
    [daypart_text] VARCHAR (63) NOT NULL,
    [total_hours]  FLOAT (53)   NOT NULL,
    CONSTRAINT [PK_dayparts] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [ind_uniq_dayparts_daypart_text]
    ON [dbo].[dayparts]([daypart_text] ASC) WITH (FILLFACTOR = 90);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains groupings of days and times.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Day Parts', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts', @level2type = N'COLUMN', @level2name = N'timespan_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts', @level2type = N'COLUMN', @level2name = N'timespan_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Short name representing day part.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Full name representing day part.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts', @level2type = N'COLUMN', @level2name = N'tier';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts', @level2type = N'COLUMN', @level2name = N'tier';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts', @level2type = N'COLUMN', @level2name = N'daypart_text';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts', @level2type = N'COLUMN', @level2name = N'daypart_text';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts', @level2type = N'COLUMN', @level2name = N'total_hours';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dayparts', @level2type = N'COLUMN', @level2name = N'total_hours';

