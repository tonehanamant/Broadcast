CREATE TABLE [dbo].[cluster_allocation_groups] (
    [id]               INT        IDENTITY (1, 1) NOT NULL,
    [traffic_id]       INT        NOT NULL,
    [cluster_id]       INT        NOT NULL,
    [mvpd_business_id] INT        NOT NULL,
    [subscribers]      FLOAT (53) NOT NULL,
    PRIMARY KEY NONCLUSTERED ([id] ASC),
    CONSTRAINT [FK_cluster_allocation_groups_businesses] FOREIGN KEY ([mvpd_business_id]) REFERENCES [dbo].[businesses] ([id]),
    CONSTRAINT [FK_cluster_allocation_groups_clusters] FOREIGN KEY ([cluster_id]) REFERENCES [dbo].[clusters] ([id]),
    CONSTRAINT [FK_cluster_allocation_groups_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id])
);


GO
CREATE CLUSTERED INDEX [IX_cluster_allocation_groups_clustered_index]
    ON [dbo].[cluster_allocation_groups]([traffic_id] ASC, [id] ASC);

