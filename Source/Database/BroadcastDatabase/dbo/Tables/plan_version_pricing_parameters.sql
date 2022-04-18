CREATE TABLE [dbo].[plan_version_pricing_parameters] (
    [id]                          INT             IDENTITY (1, 1) NOT NULL,
    [plan_version_id]             INT             NULL,
    [min_cpm]                     DECIMAL (19, 4) NULL,
    [max_cpm]                     DECIMAL (19, 4) NULL,
    [coverage_goal]               FLOAT (53)      NOT NULL,
    [impressions_goal]            FLOAT (53)      NOT NULL,
    [budget_goal]                 DECIMAL (19, 4) NOT NULL,
    [cpm_goal]                    DECIMAL (19, 4) NOT NULL,
    [proprietary_blend]           FLOAT (53)      NOT NULL,
    [competition_factor]          FLOAT (53)      NULL,
    [inflation_factor]            FLOAT (53)      NULL,
    [unit_caps_type]              INT             NOT NULL,
    [unit_caps]                   INT             NOT NULL,
    [cpp]                         DECIMAL (19, 4) NOT NULL,
    [currency]                    INT             NOT NULL,
    [rating_points]               FLOAT (53)      NOT NULL,
    [margin]                      FLOAT (53)      NULL,
    [plan_version_pricing_job_id] INT             NULL,
    [budget_adjusted]             DECIMAL (19, 4) NOT NULL,
    [cpm_adjusted]                DECIMAL (19, 4) NOT NULL,
    [market_group]                INT             NOT NULL,
    [posting_type]                INT             NOT NULL,
    [budget_cpm_lever] INT NOT NULL, 
    [fluidity_percentage] FLOAT NULL, 
    [category] INT NULL, 
    [fluidity_child_category] INT NULL, 
    CONSTRAINT [PK_plan_version_pricing_parameters] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_pricing_parameters_plan_version_pricing_job] FOREIGN KEY ([plan_version_pricing_job_id]) REFERENCES [dbo].[plan_version_pricing_job] ([id]),
    CONSTRAINT [FK_plan_version_pricing_parameters_plan_versions] FOREIGN KEY ([plan_version_id]) REFERENCES [dbo].[plan_versions] ([id])
);




GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_pricing_parameters_plan_versions]
    ON [dbo].[plan_version_pricing_parameters]([plan_version_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_pricing_parameters_plan_version_pricing_job]
    ON [dbo].[plan_version_pricing_parameters]([plan_version_pricing_job_id] ASC);

