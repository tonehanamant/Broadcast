CREATE TABLE [dbo].[scx_generation_jobs] (
    [id]                  INT          IDENTITY (1, 1) NOT NULL,
    [inventory_source_id] INT          NOT NULL,
    [start_date]          DATETIME     NOT NULL,
    [end_date]            DATETIME     NOT NULL,
    [status]              INT          NOT NULL,
    [queued_at]           DATETIME     NOT NULL,
    [completed_at]        DATETIME     NULL,
    [requested_by]        VARCHAR (63) NOT NULL,
    [standard_daypart_id] INT          NOT NULL,
    CONSTRAINT [PK_scx_generation_jobs] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_scx_generation_jobs_inventory_sources] FOREIGN KEY ([inventory_source_id]) REFERENCES [dbo].[inventory_sources] ([id]),
    CONSTRAINT [FK_scx_generation_jobs_standard_dayparts] FOREIGN KEY ([standard_daypart_id]) REFERENCES [dbo].[standard_dayparts] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_scx_generation_jobs_standard_dayparts]
    ON [dbo].[scx_generation_jobs]([standard_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_scx_generation_jobs_inventory_sources]
    ON [dbo].[scx_generation_jobs]([inventory_source_id] ASC);

