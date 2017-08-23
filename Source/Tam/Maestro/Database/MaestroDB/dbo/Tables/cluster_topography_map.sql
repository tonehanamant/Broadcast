CREATE TABLE [dbo].[cluster_topography_map] (
    [cluster_id]    INT NOT NULL,
    [topography_id] INT NOT NULL,
    CONSTRAINT [FK_cluster_topography_map_cluster_topography_map2] FOREIGN KEY ([topography_id]) REFERENCES [dbo].[topographies] ([id]),
    CONSTRAINT [FK_cluster_topography_map_clusters1] FOREIGN KEY ([cluster_id]) REFERENCES [dbo].[clusters] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cluster_topography_map';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cluster_topography_map';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cluster_topography_map';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cluster_topography_map', @level2type = N'COLUMN', @level2name = N'cluster_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cluster_topography_map', @level2type = N'COLUMN', @level2name = N'cluster_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cluster_topography_map', @level2type = N'COLUMN', @level2name = N'topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'cluster_topography_map', @level2type = N'COLUMN', @level2name = N'topography_id';

