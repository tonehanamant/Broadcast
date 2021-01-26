CREATE TABLE [dbo].[plan_version_pricing_bands] (
    [id]                          INT             IDENTITY (1, 1) NOT NULL,
    [plan_version_pricing_job_id] INT             NULL,
    [total_spots]                 INT             NOT NULL,
    [total_impressions]           FLOAT (53)      NOT NULL,
    [total_cpm]                   DECIMAL (19, 4) NOT NULL,
    [total_budget]                DECIMAL (19, 4) NOT NULL,
    [posting_type]                INT             NOT NULL,
    [spot_allocation_model_mode]  INT             NOT NULL,
    CONSTRAINT [PK_plan_version_pricing_bands] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_pricing_bands_plan_version_pricing_job] FOREIGN KEY ([plan_version_pricing_job_id]) REFERENCES [dbo].[plan_version_pricing_job] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_pricing_bands_plan_version_pricing_job]
    ON [dbo].[plan_version_pricing_bands]([plan_version_pricing_job_id] ASC);

