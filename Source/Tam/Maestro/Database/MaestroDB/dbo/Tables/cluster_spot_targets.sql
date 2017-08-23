CREATE TABLE [dbo].[cluster_spot_targets] (
    [id]                          INT        IDENTITY (1, 1) NOT NULL,
    [traffic_id]                  INT        NOT NULL,
    [cluster_allocation_group_id] INT        NOT NULL,
    [media_week_id]               INT        NOT NULL,
    [daypart_id]                  INT        NOT NULL,
    [start_date]                  DATE       NOT NULL,
    [end_date]                    DATE       NOT NULL,
    [spots]                       SMALLINT   NOT NULL,
    [fixed_spot_cost]             MONEY      NULL,
    [calculated_spot_cost]        MONEY      NOT NULL,
    [spot_cost]                   MONEY      NOT NULL,
    [impressions_per_spot]        FLOAT (53) NOT NULL,
    [calculated_spot_cost1]       MONEY      DEFAULT ((0)) NOT NULL,
    [calculated_spot_cost2]       MONEY      NULL,
    PRIMARY KEY NONCLUSTERED ([id] ASC),
    CONSTRAINT [FK_cluster_spot_targets_cluster_allocation_groups] FOREIGN KEY ([cluster_allocation_group_id]) REFERENCES [dbo].[cluster_allocation_groups] ([id]),
    CONSTRAINT [FK_cluster_spot_targets_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_cluster_spot_targets_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_cluster_spot_targets_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id])
);


GO
CREATE CLUSTERED INDEX [IX_cluster_spot_targets_clustered_index]
    ON [dbo].[cluster_spot_targets]([traffic_id] ASC, [id] ASC);

