CREATE TABLE [dbo].[plan_version_summary_quarters] (
    [id]                      INT IDENTITY (1, 1) NOT NULL,
    [plan_version_summary_id] INT NOT NULL,
    [quarter]                 INT NOT NULL,
    [year]                    INT NOT NULL,
    CONSTRAINT [PK_plan_version_summary_quarters] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_summary_quarters_plan_summary] FOREIGN KEY ([plan_version_summary_id]) REFERENCES [dbo].[plan_version_summaries] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_summary_quarters_plan_summary]
    ON [dbo].[plan_version_summary_quarters]([plan_version_summary_id] ASC);

