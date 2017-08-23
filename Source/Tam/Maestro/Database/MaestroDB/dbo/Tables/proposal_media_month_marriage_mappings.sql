CREATE TABLE [dbo].[proposal_media_month_marriage_mappings] (
    [proposal_id]    INT NOT NULL,
    [media_month_id] INT NOT NULL,
    [spot_length_id] INT NOT NULL,
    CONSTRAINT [PK_proposal_media_month_marriage_mappings_2] PRIMARY KEY CLUSTERED ([proposal_id] ASC),
    CONSTRAINT [FK_proposal_media_month_marriage_mappings_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_proposal_media_month_marriage_mappings_proposals] FOREIGN KEY ([proposal_id]) REFERENCES [dbo].[proposals] ([id]),
    CONSTRAINT [FK_proposal_media_month_marriage_mappings_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_media_month_marriage_mappings';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_media_month_marriage_mappings';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_media_month_marriage_mappings';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_media_month_marriage_mappings', @level2type = N'COLUMN', @level2name = N'proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_media_month_marriage_mappings', @level2type = N'COLUMN', @level2name = N'proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_media_month_marriage_mappings', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_media_month_marriage_mappings', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_media_month_marriage_mappings', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_media_month_marriage_mappings', @level2type = N'COLUMN', @level2name = N'spot_length_id';

