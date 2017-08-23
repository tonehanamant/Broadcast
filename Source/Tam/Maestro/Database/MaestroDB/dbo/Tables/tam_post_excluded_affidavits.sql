CREATE TABLE [dbo].[tam_post_excluded_affidavits] (
    [id]                   INT IDENTITY (1, 1) NOT NULL,
    [tam_post_id]          INT NOT NULL,
    [tam_post_proposal_id] INT NULL,
    [material_id]          INT NULL,
    [network_id]           INT NULL,
    CONSTRAINT [PK_tam_post_excluded_affidavits] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_tam_post_excluded_affidavits_materials] FOREIGN KEY ([material_id]) REFERENCES [dbo].[materials] ([id]),
    CONSTRAINT [FK_tam_post_excluded_affidavits_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_tam_post_excluded_affidavits_tam_post_proposals] FOREIGN KEY ([tam_post_proposal_id]) REFERENCES [dbo].[tam_post_proposals] ([id]),
    CONSTRAINT [FK_tam_post_excluded_affidavits_tam_posts] FOREIGN KEY ([tam_post_id]) REFERENCES [dbo].[tam_posts] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains instructions for a post as to which affidavits should not count towards the post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'TAM Post Excluded Affidavits', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Used to change the "enabled" field in the "posted_affidavits" table to false thus signaling the post aggregation process to exclude that spot.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavits', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavits', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavits', @level2type = N'COLUMN', @level2name = N'tam_post_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavits', @level2type = N'COLUMN', @level2name = N'tam_post_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the proposal within the post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavits', @level2type = N'COLUMN', @level2name = N'tam_post_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'If not null then only spots for this proposal should be disabled.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavits', @level2type = N'COLUMN', @level2name = N'tam_post_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the material.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavits', @level2type = N'COLUMN', @level2name = N'material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'If not null all spots with this material should be disabled.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavits', @level2type = N'COLUMN', @level2name = N'material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavits', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavits', @level2type = N'COLUMN', @level2name = N'network_id';

