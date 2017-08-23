CREATE TABLE [dbo].[hispanic_post_network_substitutions] (
    [network_id]               INT        NOT NULL,
    [substitution_category_id] INT        NOT NULL,
    [substitute_network_id]    INT        NOT NULL,
    [weight]                   FLOAT (53) NOT NULL,
    [effective_date]           DATETIME   NOT NULL,
    [rating_category_group_id] TINYINT    CONSTRAINT [DF_hispanic_post_network_substitutions_rating_category_group_id] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_hispanic_post_network_substitutions_1] PRIMARY KEY CLUSTERED ([network_id] ASC, [substitution_category_id] ASC),
    CONSTRAINT [FK_hispanic_post_network_substitutions_networks2] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_hispanic_post_network_substitutions_networks3] FOREIGN KEY ([substitute_network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_hispanic_post_network_substitutions_rating_category_groups] FOREIGN KEY ([rating_category_group_id]) REFERENCES [dbo].[rating_category_groups] ([id]),
    CONSTRAINT [FK_hispanic_post_network_substitutions_substitution_categories1] FOREIGN KEY ([substitution_category_id]) REFERENCES [dbo].[substitution_categories] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions', @level2type = N'COLUMN', @level2name = N'substitution_category_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions', @level2type = N'COLUMN', @level2name = N'substitution_category_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions', @level2type = N'COLUMN', @level2name = N'substitute_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions', @level2type = N'COLUMN', @level2name = N'substitute_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions', @level2type = N'COLUMN', @level2name = N'weight';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions', @level2type = N'COLUMN', @level2name = N'weight';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions', @level2type = N'COLUMN', @level2name = N'rating_category_group_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'hispanic_post_network_substitutions', @level2type = N'COLUMN', @level2name = N'rating_category_group_id';

