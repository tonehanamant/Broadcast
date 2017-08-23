CREATE TABLE [dbo].[rate_card_details] (
    [id]                  INT   IDENTITY (1, 1) NOT NULL,
    [rate_card_id]        INT   NOT NULL,
    [network_id]          INT   NOT NULL,
    [tier]                INT   NOT NULL,
    [primary_audience_id] INT   NOT NULL,
    [rate_15]             MONEY NOT NULL,
    [rate_30]             MONEY NOT NULL,
    [rate_60]             MONEY NOT NULL,
    [net_cost_hh_cpm]     MONEY NOT NULL,
    [net_cost_demo_cpm]   MONEY NOT NULL,
    CONSTRAINT [PK_rate_card_details] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_rate_card_details_audiences] FOREIGN KEY ([primary_audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_rate_card_details_networks1] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_rate_card_details_rate_cards1] FOREIGN KEY ([rate_card_id]) REFERENCES [dbo].[rate_cards] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'rate_card_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'rate_card_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'tier';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'tier';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'primary_audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'primary_audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'rate_15';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'rate_15';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'rate_30';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'rate_30';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'rate_60';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'rate_60';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'net_cost_hh_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'net_cost_hh_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'net_cost_demo_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_details', @level2type = N'COLUMN', @level2name = N'net_cost_demo_cpm';

