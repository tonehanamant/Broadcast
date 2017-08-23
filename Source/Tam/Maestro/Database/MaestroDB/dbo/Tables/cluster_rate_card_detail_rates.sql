CREATE TABLE [dbo].[cluster_rate_card_detail_rates] (
    [cluster_rate_card_detail_id] INT   NOT NULL,
    [spot_length_id]              INT   NOT NULL,
    [rate]                        MONEY NOT NULL,
    CONSTRAINT [PK_cluster_rate_card_detail_rates] PRIMARY KEY CLUSTERED ([cluster_rate_card_detail_id] ASC, [spot_length_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_cluster_rate_card_detail_rates_cluster_rate_card_details] FOREIGN KEY ([cluster_rate_card_detail_id]) REFERENCES [dbo].[cluster_rate_card_details] ([id]),
    CONSTRAINT [FK_cluster_rate_card_detail_rates_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])
);

