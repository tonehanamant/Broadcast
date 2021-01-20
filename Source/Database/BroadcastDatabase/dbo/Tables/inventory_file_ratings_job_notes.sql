CREATE TABLE [dbo].[inventory_file_ratings_job_notes] (
    [id]                            INT           IDENTITY (1, 1) NOT NULL,
    [inventory_file_ratings_job_id] INT           NOT NULL,
    [text]                          VARCHAR (MAX) NOT NULL,
    [created_at]                    DATETIME      NOT NULL,
    CONSTRAINT [PK_inventory_file_ratings_job_notes] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_file_ratings_job_notes_inventory_file_ratings_jobs] FOREIGN KEY ([inventory_file_ratings_job_id]) REFERENCES [dbo].[inventory_file_ratings_jobs] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_file_ratings_job_notes_inventory_file_ratings_jobs]
    ON [dbo].[inventory_file_ratings_job_notes]([inventory_file_ratings_job_id] ASC);

