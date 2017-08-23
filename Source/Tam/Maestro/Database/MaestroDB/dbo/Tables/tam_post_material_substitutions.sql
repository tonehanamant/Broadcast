CREATE TABLE [dbo].[tam_post_material_substitutions] (
    [tam_post_id]            INT NOT NULL,
    [material_id]            INT NOT NULL,
    [substitute_material_id] INT NOT NULL,
    CONSTRAINT [PK_tam_post_material_substitutions_1] PRIMARY KEY CLUSTERED ([tam_post_id] ASC, [material_id] ASC, [substitute_material_id] ASC),
    CONSTRAINT [FK_tam_post_material_substitutions_materials] FOREIGN KEY ([material_id]) REFERENCES [dbo].[materials] ([id]),
    CONSTRAINT [FK_tam_post_material_substitutions_materials1] FOREIGN KEY ([substitute_material_id]) REFERENCES [dbo].[materials] ([id]),
    CONSTRAINT [FK_tam_post_material_substitutions_tam_posts] FOREIGN KEY ([tam_post_id]) REFERENCES [dbo].[tam_posts] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains substitutions to be used when printing post pages involved with ISCI''s.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_material_substitutions';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'TAM Post Material Substitutions', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_material_substitutions';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Used to represent one ISCI as another during the printing process only.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_material_substitutions';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_material_substitutions', @level2type = N'COLUMN', @level2name = N'tam_post_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_material_substitutions', @level2type = N'COLUMN', @level2name = N'tam_post_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the posted material to substitute.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_material_substitutions', @level2type = N'COLUMN', @level2name = N'material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_material_substitutions', @level2type = N'COLUMN', @level2name = N'material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the substituted material.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_material_substitutions', @level2type = N'COLUMN', @level2name = N'substitute_material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_material_substitutions', @level2type = N'COLUMN', @level2name = N'substitute_material_id';

