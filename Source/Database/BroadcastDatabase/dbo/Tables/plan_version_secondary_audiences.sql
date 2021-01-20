CREATE TABLE [dbo].[plan_version_secondary_audiences] (
    [id]              INT             IDENTITY (1, 1) NOT NULL,
    [audience_type]   INT             NOT NULL,
    [audience_id]     INT             NOT NULL,
    [vpvh]            FLOAT (53)      NOT NULL,
    [cpm]             DECIMAL (19, 4) NOT NULL,
    [cpp]             FLOAT (53)      NOT NULL,
    [universe]        FLOAT (53)      NOT NULL,
    [plan_version_id] INT             NOT NULL,
    [rating_points]   FLOAT (53)      NOT NULL,
    [impressions]     FLOAT (53)      NOT NULL,
    CONSTRAINT [PK_plan_version_secondary_audiences] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_secondary_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_plan_version_secondary_audiences_plan_versions] FOREIGN KEY ([plan_version_id]) REFERENCES [dbo].[plan_versions] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_secondary_audiences_plan_versions]
    ON [dbo].[plan_version_secondary_audiences]([plan_version_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_secondary_audiences_audiences]
    ON [dbo].[plan_version_secondary_audiences]([audience_id] ASC);

