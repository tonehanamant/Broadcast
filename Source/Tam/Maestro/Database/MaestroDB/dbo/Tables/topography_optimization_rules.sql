CREATE TABLE [dbo].[topography_optimization_rules] (
    [id]                   INT          IDENTITY (1, 1) NOT NULL,
    [topography_id]        INT          NOT NULL,
    [target_topography_id] INT          NOT NULL,
    [optimization_ratio]   FLOAT (53)   NOT NULL,
    [rule_set]             VARCHAR (63) NOT NULL,
    CONSTRAINT [FK_topography_optimization_rules_topographies] FOREIGN KEY ([topography_id]) REFERENCES [dbo].[topographies] ([id]),
    CONSTRAINT [FK_topography_optimization_rules_topographies1] FOREIGN KEY ([target_topography_id]) REFERENCES [dbo].[topographies] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_optimization_rules';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_optimization_rules';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_optimization_rules';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_optimization_rules', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_optimization_rules', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_optimization_rules', @level2type = N'COLUMN', @level2name = N'topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_optimization_rules', @level2type = N'COLUMN', @level2name = N'topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_optimization_rules', @level2type = N'COLUMN', @level2name = N'target_topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_optimization_rules', @level2type = N'COLUMN', @level2name = N'target_topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_optimization_rules', @level2type = N'COLUMN', @level2name = N'optimization_ratio';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_optimization_rules', @level2type = N'COLUMN', @level2name = N'optimization_ratio';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_optimization_rules', @level2type = N'COLUMN', @level2name = N'rule_set';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'topography_optimization_rules', @level2type = N'COLUMN', @level2name = N'rule_set';

