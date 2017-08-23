CREATE TABLE [dbo].[material_medias] (
    [media_id]    INT NOT NULL,
    [material_id] INT NOT NULL,
    CONSTRAINT [PK_material_medias] PRIMARY KEY CLUSTERED ([media_id] ASC, [material_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_material_medias_medias] FOREIGN KEY ([media_id]) REFERENCES [dbo].[medias] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'material_medias';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'material_medias';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'material_medias';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'material_medias', @level2type = N'COLUMN', @level2name = N'media_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'material_medias', @level2type = N'COLUMN', @level2name = N'media_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'material_medias', @level2type = N'COLUMN', @level2name = N'material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'material_medias', @level2type = N'COLUMN', @level2name = N'material_id';

