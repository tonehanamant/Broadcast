CREATE TABLE [dbo].[plan_version_audience_daypart_vpvh] (
    [id]                  INT        IDENTITY (1, 1) NOT NULL,
    [plan_version_id]     INT        NOT NULL,
    [audience_id]         INT        NOT NULL,
    [vpvh_type]           INT        NOT NULL,
    [vpvh_value]          FLOAT (53) NOT NULL,
    [starting_point]      DATETIME   NOT NULL,
    [standard_daypart_id] INT        NOT NULL,
    [daypart_customization_id] INT NULL, 
    CONSTRAINT [PK_plan_version_audience_daypart_vpvh] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_audience_daypart_vpvh_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_plan_version_audience_daypart_vpvh_plan_versions] FOREIGN KEY ([plan_version_id]) REFERENCES [dbo].[plan_versions] ([id]),
    CONSTRAINT [FK_plan_version_audience_daypart_vpvh_standard_dayparts] FOREIGN KEY ([standard_daypart_id]) REFERENCES [dbo].[standard_dayparts] ([id]),
     CONSTRAINT [FK_ plan_version_audience_daypart_vpvh_plan_version_daypart_customizations]
    FOREIGN KEY([daypart_customization_id])REFERENCES [dbo].[plan_version_daypart_customizations] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_audience_daypart_vpvh_standard_dayparts]
    ON [dbo].[plan_version_audience_daypart_vpvh]([standard_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_audience_daypart_vpvh_plan_versions]
    ON [dbo].[plan_version_audience_daypart_vpvh]([plan_version_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_audience_daypart_vpvh_audiences]
    ON [dbo].[plan_version_audience_daypart_vpvh]([audience_id] ASC);

