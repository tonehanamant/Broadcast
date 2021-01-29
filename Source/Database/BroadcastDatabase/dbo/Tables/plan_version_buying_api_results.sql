CREATE TABLE [dbo].[plan_version_buying_api_results] (
    [id]                          INT             IDENTITY (1, 1) NOT NULL,
    [optimal_cpm]                 DECIMAL (19, 4) NOT NULL,
    [plan_version_buying_job_id]  INT             NULL,
    [buying_version]              VARCHAR (10)    NOT NULL,
    [spot_allocation_model_mode]  INT             NOT NULL,
    CONSTRAINT [PK_plan_version_buying_api_results] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_buying_api_results_plan_version_buying_job] FOREIGN KEY ([plan_version_buying_job_id]) REFERENCES [dbo].[plan_version_buying_job] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_buying_api_results_plan_version_buying_job]
    ON [dbo].[plan_version_buying_api_results]([plan_version_buying_job_id] ASC);

