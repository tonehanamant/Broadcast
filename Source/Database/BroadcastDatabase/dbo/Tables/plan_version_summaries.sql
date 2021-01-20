CREATE TABLE [dbo].[plan_version_summaries] (
    [id]                                         INT           IDENTITY (1, 1) NOT NULL,
    [processing_status]                          INT           NOT NULL,
    [hiatus_days_count]                          INT           NULL,
    [active_day_count]                           INT           NULL,
    [available_market_count]                     INT           NULL,
    [available_market_total_us_coverage_percent] FLOAT (53)    NULL,
    [blackout_market_count]                      INT           NULL,
    [blackout_market_total_us_coverage_percent]  FLOAT (53)    NULL,
    [product_name]                               VARCHAR (256) NULL,
    [plan_version_id]                            INT           NOT NULL,
    [available_market_with_sov_count]            INT           NULL,
    CONSTRAINT [PK_plan_version_summaries] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_summaries_plan_versions] FOREIGN KEY ([plan_version_id]) REFERENCES [dbo].[plan_versions] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_summaries_plan_versions]
    ON [dbo].[plan_version_summaries]([plan_version_id] ASC);

