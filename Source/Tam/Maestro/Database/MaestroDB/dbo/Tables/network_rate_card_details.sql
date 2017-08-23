CREATE TABLE [dbo].[network_rate_card_details] (
    [id]                     INT        IDENTITY (1, 1) NOT NULL,
    [network_rate_card_id]   INT        NOT NULL,
    [tier]                   INT        NOT NULL,
    [network_id]             INT        NOT NULL,
    [primary_audience_id]    INT        NULL,
    [hh_us_universe]         FLOAT (53) NOT NULL,
    [hh_coverage_universe]   FLOAT (53) NOT NULL,
    [hh_cpm]                 MONEY      NOT NULL,
    [hh_rating]              FLOAT (53) NOT NULL,
    [hh_delivery]            FLOAT (53) NOT NULL,
    [hh_net_cost_cpm]        MONEY      NOT NULL,
    [demo_us_universe]       FLOAT (53) NULL,
    [demo_coverage_universe] FLOAT (53) NULL,
    [demo_cpm]               MONEY      NULL,
    [demo_rating]            FLOAT (53) NULL,
    [demo_delivery]          FLOAT (53) NULL,
    [demo_net_cost_cpm]      MONEY      NULL,
    [lock_rate]              BIT        CONSTRAINT [DF_network_rate_card_details_lock_rate] DEFAULT ((0)) NOT NULL,
    [minimum_cpm]            MONEY      DEFAULT (NULL) NULL,
    CONSTRAINT [PK_network_rating_details] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_network_rate_card_details_audiences] FOREIGN KEY ([primary_audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_network_rate_card_details_network_rate_cards] FOREIGN KEY ([network_rate_card_id]) REFERENCES [dbo].[network_rate_cards] ([id]),
    CONSTRAINT [FK_network_rate_card_details_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'network_rate_card_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'network_rate_card_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'tier';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'tier';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'primary_audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'primary_audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'hh_us_universe';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'hh_us_universe';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'hh_coverage_universe';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'hh_coverage_universe';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'hh_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'hh_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'hh_rating';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'hh_rating';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'hh_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'hh_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'hh_net_cost_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'hh_net_cost_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'demo_us_universe';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'demo_us_universe';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'demo_coverage_universe';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'demo_coverage_universe';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'demo_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'demo_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'demo_rating';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'demo_rating';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'demo_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'demo_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'demo_net_cost_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'demo_net_cost_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'lock_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'lock_rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'minimum_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_details', @level2type = N'COLUMN', @level2name = N'minimum_cpm';

