CREATE TABLE [dbo].[tam_post_exclusion_summary_audiences] (
    [tam_post_exclusion_summary_id] INT        NOT NULL,
    [audience_id]                   INT        NOT NULL,
    [delivery]                      FLOAT (53) NOT NULL,
    [eq_delivery]                   FLOAT (53) NOT NULL,
    [dr_delivery]                   FLOAT (53) NOT NULL,
    [dr_eq_delivery]                FLOAT (53) NOT NULL,
    CONSTRAINT [PK_tam_post_exclusion_summary_audiences] PRIMARY KEY CLUSTERED ([tam_post_exclusion_summary_id] ASC, [audience_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_tam_post_exclusion_summary_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_tam_post_exclusion_summary_audiences_tam_post_exclusion_summaries] FOREIGN KEY ([tam_post_exclusion_summary_id]) REFERENCES [dbo].[tam_post_exclusion_summaries] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences', @level2type = N'COLUMN', @level2name = N'tam_post_exclusion_summary_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences', @level2type = N'COLUMN', @level2name = N'tam_post_exclusion_summary_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences', @level2type = N'COLUMN', @level2name = N'delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences', @level2type = N'COLUMN', @level2name = N'delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences', @level2type = N'COLUMN', @level2name = N'eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences', @level2type = N'COLUMN', @level2name = N'eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences', @level2type = N'COLUMN', @level2name = N'dr_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences', @level2type = N'COLUMN', @level2name = N'dr_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_exclusion_summary_audiences';

