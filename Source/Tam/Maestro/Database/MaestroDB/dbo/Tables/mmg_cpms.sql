CREATE TABLE [dbo].[mmg_cpms] (
    [daypart_id]         INT     NOT NULL,
    [network_group_type] TINYINT NOT NULL,
    [cpm]                MONEY   NOT NULL,
    CONSTRAINT [PK_mmg_cpms] PRIMARY KEY CLUSTERED ([daypart_id] ASC, [network_group_type] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_mmg_cpms_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'mmg_cpms';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'mmg_cpms';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'mmg_cpms';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'mmg_cpms', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'mmg_cpms', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'mmg_cpms', @level2type = N'COLUMN', @level2name = N'network_group_type';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'mmg_cpms', @level2type = N'COLUMN', @level2name = N'network_group_type';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'mmg_cpms', @level2type = N'COLUMN', @level2name = N'cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'mmg_cpms', @level2type = N'COLUMN', @level2name = N'cpm';

