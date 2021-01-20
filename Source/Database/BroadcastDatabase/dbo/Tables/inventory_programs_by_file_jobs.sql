CREATE TABLE [dbo].[inventory_programs_by_file_jobs] (
    [id]                INT           IDENTITY (1, 1) NOT NULL,
    [status]            INT           NOT NULL,
    [inventory_file_id] INT           NOT NULL,
    [queued_at]         DATETIME      NOT NULL,
    [queued_by]         VARCHAR (63)  NOT NULL,
    [completed_at]      DATETIME      NULL,
    [status_message]    VARCHAR (200) NULL,
    CONSTRAINT [PK_inventory_programs_by_file_jobs] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_programs_by_file_jobs_inventory_file] FOREIGN KEY ([inventory_file_id]) REFERENCES [dbo].[inventory_files] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_programs_by_file_jobs_inventory_file]
    ON [dbo].[inventory_programs_by_file_jobs]([inventory_file_id] ASC);

