CREATE TABLE [dbo].[pricing_guide_distribution_proprietary_inventory] (
    [id]                            INT             IDENTITY (1, 1) NOT NULL,
    [pricing_guide_distribution_id] INT             NOT NULL,
    [inventory_source]              TINYINT         NOT NULL,
    [impressions_balance_percent]   FLOAT (53)      NOT NULL,
    [cpm]                           DECIMAL (19, 4) NOT NULL,
    CONSTRAINT [PK_pricing_guide_distribution_proprietary_inventory] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_pricing_guide_distribution_proprietary_inventory_pricing_guide_distributions] FOREIGN KEY ([pricing_guide_distribution_id]) REFERENCES [dbo].[pricing_guide_distributions] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_pricing_guide_distribution_proprietary_inventory_pricing_guide_distributions]
    ON [dbo].[pricing_guide_distribution_proprietary_inventory]([pricing_guide_distribution_id] ASC);

