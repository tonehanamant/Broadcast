CREATE TABLE [dbo].[plan_version_pricing_results] (
    [id]                            INT             IDENTITY (1, 1) NOT NULL,
    [optimal_cpm]                   DECIMAL (19, 4) NOT NULL,
    [total_market_count]            INT             NOT NULL,
    [total_station_count]           INT             NOT NULL,
    [total_avg_cpm]                 DECIMAL (19, 4) NOT NULL,
    [total_avg_impressions]         FLOAT (53)      NOT NULL,
    [goal_fulfilled_by_proprietary] BIT             NOT NULL,
    [total_impressions]             FLOAT (53)      NOT NULL,
    [total_budget]                  DECIMAL (19, 4) NOT NULL,
    [plan_version_pricing_job_id]   INT             NULL,
    [total_spots]                   INT             NOT NULL,
    [posting_type]                  INT             NOT NULL,
    CONSTRAINT [PK_plan_version_pricing_results] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_pricing_results_plan_version_pricing_job] FOREIGN KEY ([plan_version_pricing_job_id]) REFERENCES [dbo].[plan_version_pricing_job] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_pricing_results_plan_version_pricing_job]
    ON [dbo].[plan_version_pricing_results]([plan_version_pricing_job_id] ASC);

