CREATE TABLE [dbo].[tam_post_proposal_gap_projections] (
    [tam_post_proposal_id] INT        NOT NULL,
    [rate_card_type_id]    INT        NOT NULL,
    [gap_projection]       FLOAT (53) NOT NULL,
    CONSTRAINT [PK_tam_post_proposal_gap_projections] PRIMARY KEY CLUSTERED ([tam_post_proposal_id] ASC, [rate_card_type_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_tam_post_proposal_gap_projections_rate_card_types] FOREIGN KEY ([rate_card_type_id]) REFERENCES [dbo].[rate_card_types] ([id]),
    CONSTRAINT [FK_tam_post_proposal_gap_projections_tam_post_proposals] FOREIGN KEY ([tam_post_proposal_id]) REFERENCES [dbo].[tam_post_proposals] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposal_gap_projections';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposal_gap_projections';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposal_gap_projections';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposal_gap_projections', @level2type = N'COLUMN', @level2name = N'tam_post_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposal_gap_projections', @level2type = N'COLUMN', @level2name = N'tam_post_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposal_gap_projections', @level2type = N'COLUMN', @level2name = N'rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Enum', @value = N'RateCardTypeEnum', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposal_gap_projections', @level2type = N'COLUMN', @level2name = N'rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposal_gap_projections', @level2type = N'COLUMN', @level2name = N'rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposal_gap_projections', @level2type = N'COLUMN', @level2name = N'gap_projection';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposal_gap_projections', @level2type = N'COLUMN', @level2name = N'gap_projection';

