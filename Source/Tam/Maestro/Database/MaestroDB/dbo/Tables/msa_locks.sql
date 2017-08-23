CREATE TABLE [dbo].[msa_locks] (
    [media_month_id] INT NOT NULL,
    [is_locked]      BIT NOT NULL,
    CONSTRAINT [PK_msa_locks] PRIMARY KEY CLUSTERED ([media_month_id] ASC),
    CONSTRAINT [FK_msa_locks_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id])
);

