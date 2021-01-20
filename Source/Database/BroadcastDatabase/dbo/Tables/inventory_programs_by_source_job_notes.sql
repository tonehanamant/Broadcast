CREATE TABLE [dbo].[inventory_programs_by_source_job_notes] (
    [id]         INT           IDENTITY (1, 1) NOT NULL,
    [job_id]     INT           NOT NULL,
    [text]       VARCHAR (MAX) NOT NULL,
    [created_at] DATETIME      NOT NULL,
    CONSTRAINT [PK_inventory_programs_by_source_job_notes] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_programs_by_source_job_notes_job] FOREIGN KEY ([job_id]) REFERENCES [dbo].[inventory_programs_by_source_jobs] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_programs_by_source_job_notes_job]
    ON [dbo].[inventory_programs_by_source_job_notes]([job_id] ASC);

