CREATE TABLE [dbo].[plan_version_dayparts] (
    [id]                                  INT        IDENTITY (1, 1) NOT NULL,
    [start_time_seconds]                  INT        NOT NULL,
    [end_time_seconds]                    INT        NOT NULL,
    [weighting_goal_percent]              FLOAT (53) NULL,
    [daypart_type]                        INT        NOT NULL,
    [is_start_time_modified]              BIT        NOT NULL,
    [is_end_time_modified]                BIT        NOT NULL,
    [plan_version_id]                     INT        NOT NULL,
    [show_type_restrictions_contain_type] INT        NULL,
    [genre_restrictions_contain_type]     INT        NULL,
    [program_restrictions_contain_type]   INT        NULL,
    [affiliate_restrictions_contain_type] INT        NULL,
    [weekdays_weighting]                  FLOAT (53) NULL,
    [weekend_weighting]                   FLOAT (53) NULL,
    [standard_daypart_id]                 INT        NOT NULL,
    CONSTRAINT [PK_plan_version_dayparts] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_dayparts_standard_dayparts] FOREIGN KEY ([standard_daypart_id]) REFERENCES [dbo].[standard_dayparts] ([id]),
    CONSTRAINT [FK_plan_version_dayparts_plan_versions] FOREIGN KEY ([plan_version_id]) REFERENCES [dbo].[plan_versions] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_dayparts_standard_dayparts]
    ON [dbo].[plan_version_dayparts]([standard_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_dayparts_plan_versions]
    ON [dbo].[plan_version_dayparts]([plan_version_id] ASC);

