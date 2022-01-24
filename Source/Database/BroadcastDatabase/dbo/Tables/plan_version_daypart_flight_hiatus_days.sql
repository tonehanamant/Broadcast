CREATE TABLE [dbo].[plan_version_daypart_flight_hiatus_days] (
    [id]                           INT      IDENTITY (1, 1) NOT NULL,
    [plan_version_daypart_goal_id] INT      NOT NULL,
    [hiatus_day]                   DATETIME NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_daypart_flight_hiatus_days_plan_version_daypart_goals] FOREIGN KEY ([plan_version_daypart_goal_id]) REFERENCES [dbo].[plan_version_daypart_goals] ([id]) ON DELETE CASCADE
);



