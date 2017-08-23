CREATE TABLE [dbo].[release_topography_rate_card_daypart_breakdown_rules] (
    [id]                INT IDENTITY (1, 1) NOT NULL,
    [topography_id]     INT NOT NULL,
    [rate_card_type_id] INT NOT NULL,
    [daypart_id]        INT NOT NULL,
    [network_id]        INT NULL,
    CONSTRAINT [PK_release_topography_rate_card_daypart_breakdown_rules_1] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_release_topography_rate_card_daypart_breakdown_rules_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_release_topography_rate_card_daypart_breakdown_rules_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_release_topography_rate_card_daypart_breakdown_rules_rate_card_types] FOREIGN KEY ([rate_card_type_id]) REFERENCES [dbo].[rate_card_types] ([id]),
    CONSTRAINT [FK_release_topography_rate_card_daypart_breakdown_rules_topographies] FOREIGN KEY ([topography_id]) REFERENCES [dbo].[topographies] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_topography_rate_card_daypart_breakdown_rules';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_topography_rate_card_daypart_breakdown_rules';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_topography_rate_card_daypart_breakdown_rules';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_topography_rate_card_daypart_breakdown_rules', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_topography_rate_card_daypart_breakdown_rules', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_topography_rate_card_daypart_breakdown_rules', @level2type = N'COLUMN', @level2name = N'topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_topography_rate_card_daypart_breakdown_rules', @level2type = N'COLUMN', @level2name = N'topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_topography_rate_card_daypart_breakdown_rules', @level2type = N'COLUMN', @level2name = N'rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_topography_rate_card_daypart_breakdown_rules', @level2type = N'COLUMN', @level2name = N'rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_topography_rate_card_daypart_breakdown_rules', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_topography_rate_card_daypart_breakdown_rules', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_topography_rate_card_daypart_breakdown_rules', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_topography_rate_card_daypart_breakdown_rules', @level2type = N'COLUMN', @level2name = N'network_id';

