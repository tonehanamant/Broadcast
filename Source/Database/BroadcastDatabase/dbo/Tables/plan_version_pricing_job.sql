CREATE TABLE [dbo].[plan_version_pricing_job] (
    [id]                INT            IDENTITY (1, 1) NOT NULL,
    [plan_version_id]   INT            NULL,
    [status]            INT            NOT NULL,
    [queued_at]         DATETIME       NOT NULL,
    [completed_at]      DATETIME       NULL,
    [error_message]     NVARCHAR (MAX) NULL,
    [diagnostic_result] NVARCHAR (MAX) NULL,
    [hangfire_job_id]   VARCHAR (16)   NULL,
    CONSTRAINT [PK_plan_version_pricing_job] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_pricing_job_plan_versions] FOREIGN KEY ([plan_version_id]) REFERENCES [dbo].[plan_versions] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_pricing_job_plan_versions]
    ON [dbo].[plan_version_pricing_job]([plan_version_id] ASC);

