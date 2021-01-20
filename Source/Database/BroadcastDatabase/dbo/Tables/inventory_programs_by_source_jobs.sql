CREATE TABLE [dbo].[inventory_programs_by_source_jobs] (
    [id]                  INT              IDENTITY (1, 1) NOT NULL,
    [status]              INT              NOT NULL,
    [inventory_source_id] INT              NOT NULL,
    [start_date]          DATETIME         NOT NULL,
    [end_date]            DATETIME         NOT NULL,
    [queued_at]           DATETIME         NOT NULL,
    [queued_by]           VARCHAR (63)     NOT NULL,
    [completed_at]        DATETIME         NULL,
    [job_group_id]        UNIQUEIDENTIFIER NULL,
    [status_message]      VARCHAR (200)    NULL,
    CONSTRAINT [PK_inventory_programs_by_source_jobs] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_programs_by_source_jobs_inventory_source] FOREIGN KEY ([inventory_source_id]) REFERENCES [dbo].[inventory_sources] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_programs_by_source_jobs_inventory_source]
    ON [dbo].[inventory_programs_by_source_jobs]([inventory_source_id] ASC);

