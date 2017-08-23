CREATE TABLE [dbo].[traffic_rate_card_detail_rates] (
    [traffic_rate_card_detail_id] INT   NOT NULL,
    [spot_length_id]              INT   NOT NULL,
    [rate]                        MONEY NOT NULL,
    CONSTRAINT [PK_traffic_rate_card_detail_rates] PRIMARY KEY CLUSTERED ([traffic_rate_card_detail_id] ASC, [spot_length_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_rate_card_detail_rates_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id]),
    CONSTRAINT [FK_traffic_rate_card_detail_rates_traffic_rate_card_details] FOREIGN KEY ([traffic_rate_card_detail_id]) REFERENCES [dbo].[traffic_rate_card_details] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_rate_card_detail_rates';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_rate_card_detail_rates';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_rate_card_detail_rates';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_rate_card_detail_rates', @level2type = N'COLUMN', @level2name = N'traffic_rate_card_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_rate_card_detail_rates', @level2type = N'COLUMN', @level2name = N'traffic_rate_card_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_rate_card_detail_rates', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_rate_card_detail_rates', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_rate_card_detail_rates', @level2type = N'COLUMN', @level2name = N'rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_rate_card_detail_rates', @level2type = N'COLUMN', @level2name = N'rate';

