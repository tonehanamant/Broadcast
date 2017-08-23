CREATE TABLE [dbo].[cluster_network_calculations] (
    [id]                          INT        IDENTITY (1, 1) NOT NULL,
    [cluster_allocation_group_id] INT        NOT NULL,
    [network_id]                  INT        NOT NULL,
    [subscribers]                 FLOAT (53) NOT NULL,
    PRIMARY KEY NONCLUSTERED ([id] ASC),
    CONSTRAINT [FK_cluster_network_calculations_cluster_allocation_groups] FOREIGN KEY ([cluster_allocation_group_id]) REFERENCES [dbo].[cluster_allocation_groups] ([id]),
    CONSTRAINT [FK_cluster_network_calculations_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id])
);


GO
CREATE CLUSTERED INDEX [IX_cluster_network_calculations_clustered_index]
    ON [dbo].[cluster_network_calculations]([cluster_allocation_group_id] ASC, [id] ASC);

