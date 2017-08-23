CREATE TABLE [dbo].[traffic_spot_target_allocation_group] (
    [id]                      INT             NOT NULL,
    [mvpd_id]                 INT             NOT NULL,
    [network_id]              INT             NOT NULL,
    [media_week_id]           INT             NOT NULL,
    [start_date]              DATE            NOT NULL,
    [end_date]                DATE            NOT NULL,
    [traffic_id]              INT             NOT NULL,
    [traffic_detail_id]       INT             NOT NULL,
    [guaranteed_audience_id]  INT             NOT NULL,
    [traffic_gross_cpm1]      MONEY           NOT NULL,
    [traffic_gross_cpm2]      MONEY           NULL,
    [proposal_rate]           MONEY           NOT NULL,
    [proposal_goal]           FLOAT (53)      NOT NULL,
    [traffic_goal]            FLOAT (53)      NOT NULL,
    [subscribers]             FLOAT (53)      NOT NULL,
    [discount_factor]         FLOAT (53)      NOT NULL,
    [proposal_gross_cpm1]     MONEY           NOT NULL,
    [proposal_gross_cpm2]     MONEY           NULL,
    [network_index]           DECIMAL (19, 8) NULL,
    [guaranteed_audience_id1] INT             NULL,
    [guaranteed_audience_id2] INT             NULL,
    [proposal_gross_cpm]      MONEY           NOT NULL,
    [traffic_gross_cpm]       MONEY           NOT NULL,
    [mvpd_universe_ratio]     FLOAT (53)      NOT NULL,
    CONSTRAINT [PK_traffic_spot_target_allocation_group] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_spot_target_allocation_group_businesses] FOREIGN KEY ([mvpd_id]) REFERENCES [dbo].[businesses] ([id]),
    CONSTRAINT [FK_traffic_spot_target_allocation_group_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_traffic_spot_target_allocation_group_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_traffic_spot_target_allocation_group_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id]),
    CONSTRAINT [FK_traffic_spot_target_allocation_group_traffic_details] FOREIGN KEY ([traffic_detail_id]) REFERENCES [dbo].[traffic_details] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_traffic_spot_target_allocation_group_traffic_id]
    ON [dbo].[traffic_spot_target_allocation_group]([traffic_id] ASC)
    INCLUDE([id]);

