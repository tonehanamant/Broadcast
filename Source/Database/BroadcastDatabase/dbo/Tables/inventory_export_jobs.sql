CREATE TABLE [dbo].[inventory_export_jobs] (
    [id]                   INT           IDENTITY (1, 1) NOT NULL,
    [inventory_source_id]  INT           NOT NULL,
    [quarter_year]         INT           NOT NULL,
    [quarter_number]       INT           NOT NULL,
    [export_genre_type_id] INT           NOT NULL,
    [status]               INT           NOT NULL,
    [status_message]       VARCHAR (MAX) NULL,
    [file_name]            VARCHAR (200) NULL,
    [completed_at]         DATETIME      NULL,
    [created_at]           DATETIME      NOT NULL,
    [created_by]           VARCHAR (63)  NOT NULL,
    [shared_folder_files_id] UNIQUEIDENTIFIER NULL, 
    CONSTRAINT [PK_inventory_export_jobs] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_export_jobs_inventory_source] FOREIGN KEY ([inventory_source_id]) REFERENCES [dbo].[inventory_sources] ([id]),
    CONSTRAINT [FK_inventory_export_job_files_shared_folder_files_id] FOREIGN KEY ([shared_folder_files_id]) REFERENCES [dbo].[shared_folder_files] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_export_jobs_inventory_source]
    ON [dbo].[inventory_export_jobs]([inventory_source_id] ASC);

