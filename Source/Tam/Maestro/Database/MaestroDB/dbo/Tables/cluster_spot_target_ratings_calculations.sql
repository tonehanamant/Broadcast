CREATE TABLE [dbo].[cluster_spot_target_ratings_calculations] (
    [id]                                    INT        IDENTITY (1, 1) NOT NULL,
    [cluster_spot_target_id]                INT        NOT NULL,
    [audience_id]                           INT        NOT NULL,
    [network_id]                            INT        NOT NULL,
    [coverage_universe]                     FLOAT (53) NOT NULL,
    [impressions_per_spot]                  FLOAT (53) NOT NULL,
    [scaling_factor]                        FLOAT (53) NOT NULL,
    [traffic_rating]                        FLOAT (53) NOT NULL,
    [traffic_detail_id]                     INT        NOT NULL,
    [subscribers]                           FLOAT (53) NOT NULL,
    [impressions_per_spot_before_prorating] FLOAT (53) NOT NULL,
    PRIMARY KEY NONCLUSTERED ([id] ASC),
    CONSTRAINT [FK_cluster_spot_target_ratings_calculations_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_cluster_spot_target_ratings_calculations_cluster_spot_targets] FOREIGN KEY ([cluster_spot_target_id]) REFERENCES [dbo].[cluster_spot_targets] ([id]),
    CONSTRAINT [FK_cluster_spot_target_ratings_calculations_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id])
);


GO
CREATE CLUSTERED INDEX [IX_cluster_spot_target_ratings_calculations_clustered_index]
    ON [dbo].[cluster_spot_target_ratings_calculations]([cluster_spot_target_id] ASC, [id] ASC);

