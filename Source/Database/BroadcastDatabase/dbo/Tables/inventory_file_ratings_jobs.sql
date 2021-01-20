CREATE TABLE [dbo].[inventory_file_ratings_jobs] (
    [id]                INT      IDENTITY (1, 1) NOT NULL,
    [inventory_file_id] INT      NOT NULL,
    [status]            INT      NOT NULL,
    [queued_at]         DATETIME NOT NULL,
    [completed_at]      DATETIME NULL,
    CONSTRAINT [PK_inventory_file_ratings_jobs] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_file_ratings_jobs_inventory_files] FOREIGN KEY ([inventory_file_id]) REFERENCES [dbo].[inventory_files] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_file_ratings_jobs_inventory_files]
    ON [dbo].[inventory_file_ratings_jobs]([inventory_file_id] ASC);

