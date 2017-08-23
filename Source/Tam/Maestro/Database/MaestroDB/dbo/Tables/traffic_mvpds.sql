CREATE TABLE [dbo].[traffic_mvpds] (
    [traffic_id]           INT NOT NULL,
    [topography_id]        INT NOT NULL,
    [traffic_rate_card_id] INT NULL,
    [business_id]          INT NOT NULL,
    [network_index_on]     BIT NOT NULL,
    [use_multi_daypart]    BIT NOT NULL,
    [cluster_rate_card_id] INT NULL,
    [is_locked]            BIT CONSTRAINT [DF_traffic_mvpds_is_locked] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_traffic_mvpds] PRIMARY KEY CLUSTERED ([traffic_id] ASC, [topography_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_mvpds_businesses] FOREIGN KEY ([business_id]) REFERENCES [dbo].[businesses] ([id]),
    CONSTRAINT [FK_traffic_mvpds_cluster_rate_card] FOREIGN KEY ([cluster_rate_card_id]) REFERENCES [dbo].[cluster_rate_cards] ([id]),
    CONSTRAINT [FK_traffic_mvpds_topographies] FOREIGN KEY ([topography_id]) REFERENCES [dbo].[topographies] ([id]),
    CONSTRAINT [FK_traffic_mvpds_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id]),
    CONSTRAINT [FK_traffic_mvpds_traffic_rate_cards] FOREIGN KEY ([traffic_rate_card_id]) REFERENCES [dbo].[traffic_rate_cards] ([id])
);

