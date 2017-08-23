CREATE TABLE [dbo].[traffic_spot_targets] (
    [id]                                      INT             NOT NULL,
    [traffic_spot_target_allocation_group_id] INT             NOT NULL,
    [traffic_id]                              INT             NOT NULL,
    [daypart_id]                              INT             NOT NULL,
    [spots]                                   SMALLINT        NOT NULL,
    [spot_cost1]                              MONEY           NOT NULL,
    [impressions_per_spot]                    FLOAT (53)      NOT NULL,
    [calculated_cost]                         MONEY           NOT NULL,
    [fixed_cost]                              MONEY           NULL,
    [spot_cost2]                              MONEY           NULL,
    [original_spots]                          DECIMAL (19, 8) NOT NULL,
    [suspended]                               BIT             CONSTRAINT [DF_traffic_spot_targets_suspended] DEFAULT ((0)) NOT NULL,
    [spot_cost]                               MONEY           NOT NULL,
    [calculated_cost1]                        MONEY           NOT NULL,
    [calculated_cost2]                        MONEY           NULL,
    [fixed_rate_cpm]                          MONEY           NULL,
    [fixed_rate_cpm1]                         MONEY           NULL,
    [fixed_rate_cpm2]                         MONEY           NULL,
    [spots_before_cap]                        INT             NULL,
    [minimum_spot_cost]                       MONEY           NULL,
    [is_below_minimum_spot_cost]              BIT             DEFAULT ((0)) NOT NULL,
    [override_spot_cost_with_minimum]         BIT             DEFAULT ((0)) NOT NULL,
    [suspended_by_traffic_alert_id]           INT             NULL,
    CONSTRAINT [PK_traffic_spot_targets] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_spot_targets_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_traffic_spot_targets_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id]),
    CONSTRAINT [FK_traffic_spot_targets_traffic_spot_target_allocation_group] FOREIGN KEY ([traffic_spot_target_allocation_group_id]) REFERENCES [dbo].[traffic_spot_target_allocation_group] ([id])
);




GO
CREATE NONCLUSTERED INDEX [IX_traffic_spot_targets_traffic_id]
    ON [dbo].[traffic_spot_targets]([traffic_id] ASC)
    INCLUDE([id]);


GO
CREATE NONCLUSTERED INDEX [IX_traffic_spot_targets_traffic_spot_target_allocation_group_id]
    ON [dbo].[traffic_spot_targets]([traffic_spot_target_allocation_group_id] ASC);

