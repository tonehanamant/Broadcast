﻿CREATE TABLE [dbo].[plan_version_pricing_band_details] (
    [id]                             INT             IDENTITY (1, 1) NOT NULL,
    [plan_version_pricing_band_id]   INT             NOT NULL,
    [min_band]                       DECIMAL (19, 4) NULL,
    [max_band]                       DECIMAL (19, 4) NULL,
    [spots]                          INT             NOT NULL,
    [impressions]                    FLOAT (53)      NOT NULL,
    [cpm]                            DECIMAL (19, 4) NOT NULL,
    [budget]                         DECIMAL (19, 4) NOT NULL,
    [impressions_percentage]         FLOAT (53)      NOT NULL,
    [available_inventory_percentage] FLOAT (53)      NOT NULL,
    [is_proprietary]                 BIT             NOT NULL,
    CONSTRAINT [PK_plan_version_pricing_band_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_pricing_band_details_plan_version_pricing_bands] FOREIGN KEY ([plan_version_pricing_band_id]) REFERENCES [dbo].[plan_version_pricing_bands] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_pricing_band_details_plan_version_pricing_bands]
    ON [dbo].[plan_version_pricing_band_details]([plan_version_pricing_band_id] ASC);
