CREATE TABLE [dbo].[tam_post_excluded_affidavit_systems] (
    [tam_post_excluded_affidavit_id] INT NOT NULL,
    [system_id]                      INT NOT NULL,
    CONSTRAINT [PK_tam_post_excluded_affidavit_systems] PRIMARY KEY CLUSTERED ([tam_post_excluded_affidavit_id] ASC, [system_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_tam_post_excluded_affidavit_systems_systems] FOREIGN KEY ([system_id]) REFERENCES [dbo].[systems] ([id]),
    CONSTRAINT [FK_tam_post_excluded_affidavit_systems_tam_post_excluded_affidavits] FOREIGN KEY ([tam_post_excluded_affidavit_id]) REFERENCES [dbo].[tam_post_excluded_affidavits] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains instructions (systems) for a post as to which affidavits should not count towards the post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavit_systems';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'TAM Post Excluded Affidavit Systems', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavit_systems';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Used to change the "enabled" field in the "posted_affidavits" table to false thus signaling the post aggregation process to exclude that spot.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavit_systems';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the TAM Post Excluded Affidavit record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavit_systems', @level2type = N'COLUMN', @level2name = N'tam_post_excluded_affidavit_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavit_systems', @level2type = N'COLUMN', @level2name = N'tam_post_excluded_affidavit_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the System record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavit_systems', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_excluded_affidavit_systems', @level2type = N'COLUMN', @level2name = N'system_id';

