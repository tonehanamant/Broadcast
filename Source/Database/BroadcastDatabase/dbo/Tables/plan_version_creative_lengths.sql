CREATE TABLE [dbo].[plan_version_creative_lengths] (
    [id]              INT IDENTITY (1, 1) NOT NULL,
    [plan_version_id] INT NOT NULL,
    [spot_length_id]  INT NOT NULL,
    [weight]          INT NULL,
    CONSTRAINT [PK_plan_version_creative_lengths] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_creative_lengths_plan_versions] FOREIGN KEY ([plan_version_id]) REFERENCES [dbo].[plan_versions] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_plan_version_creative_lengths_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_creative_lengths_spot_lengths]
    ON [dbo].[plan_version_creative_lengths]([spot_length_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_creative_lengths_plan_versions]
    ON [dbo].[plan_version_creative_lengths]([plan_version_id] ASC);

