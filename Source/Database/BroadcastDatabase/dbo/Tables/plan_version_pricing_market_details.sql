CREATE TABLE [dbo].[plan_version_pricing_market_details] (
    [id]                             INT          IDENTITY (1, 1) NOT NULL,
    [plan_version_pricing_market_id] INT          NOT NULL,
    [rank]                           INT          NOT NULL,
    [market_coverage_percent]        FLOAT (53)   NOT NULL,
    [stations]                       INT          NOT NULL,
    [spots]                          INT          NOT NULL,
    [impressions]                    FLOAT (53)   NOT NULL,
    [budget]                         FLOAT (53)   NOT NULL,
    [impressions_percentage]         FLOAT (53)   NOT NULL,
    [share_of_voice_goal_percentage] FLOAT (53)   NULL,
    [market_name]                    VARCHAR (31) NOT NULL,
    [is_proprietary]                 BIT          NOT NULL,
    [stations_per_market]            INT          NOT NULL,
    CONSTRAINT [PK_plan_version_pricing_market_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_pricing_market_details_pricing_market] FOREIGN KEY ([plan_version_pricing_market_id]) REFERENCES [dbo].[plan_version_pricing_markets] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_pricing_market_details_pricing_market]
    ON [dbo].[plan_version_pricing_market_details]([plan_version_pricing_market_id] ASC);

