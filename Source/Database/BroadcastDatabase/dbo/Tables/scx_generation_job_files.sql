CREATE TABLE [dbo].[scx_generation_job_files] (
    [id]                    INT           IDENTITY (1, 1) NOT NULL,
    [scx_generation_job_id] INT           NOT NULL,
    [file_name]             VARCHAR (255) NOT NULL,
    [inventory_source_id]   INT           NOT NULL,
    [start_date]            DATETIME      NOT NULL,
    [end_date]              DATETIME      NOT NULL,
    [unit_name]             VARCHAR (50)  NOT NULL,
    [standard_daypart_id]   INT           NOT NULL,
    [shared_folder_files_id] UNIQUEIDENTIFIER NULL, 
    CONSTRAINT [PK_scx_generation_job_files] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_scx_generation_job_files_inventory_sources] FOREIGN KEY ([inventory_source_id]) REFERENCES [dbo].[inventory_sources] ([id]),
    CONSTRAINT [FK_scx_generation_job_files_scx_generation_job] FOREIGN KEY ([scx_generation_job_id]) REFERENCES [dbo].[scx_generation_jobs] ([id]),
    CONSTRAINT [FK_scx_generation_job_files_standard_dayparts] FOREIGN KEY ([standard_daypart_id]) REFERENCES [dbo].[standard_dayparts] ([id]),
    CONSTRAINT [FK_scx_generation_job_files_shared_folder_files_id] FOREIGN KEY ([shared_folder_files_id]) REFERENCES [dbo].[shared_folder_files] ([id]),
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_scx_generation_job_files_standard_dayparts]
    ON [dbo].[scx_generation_job_files]([standard_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_scx_generation_job_files_scx_generation_job]
    ON [dbo].[scx_generation_job_files]([scx_generation_job_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_scx_generation_job_files_inventory_sources]
    ON [dbo].[scx_generation_job_files]([inventory_source_id] ASC);

