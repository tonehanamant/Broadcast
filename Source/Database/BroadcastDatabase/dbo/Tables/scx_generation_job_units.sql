CREATE TABLE [dbo].[scx_generation_job_units] (
    [id]                    INT          IDENTITY (1, 1) NOT NULL,
    [scx_generation_job_id] INT          NOT NULL,
    [unit_name]             VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_scx_generation_job_units] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_scx_generation_job_units_scx_generation_jobs] FOREIGN KEY ([scx_generation_job_id]) REFERENCES [dbo].[scx_generation_jobs] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_scx_generation_job_units_scx_generation_jobs]
    ON [dbo].[scx_generation_job_units]([scx_generation_job_id] ASC);

